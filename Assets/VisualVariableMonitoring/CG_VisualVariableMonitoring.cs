using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

//========================
//	Author: Cyrille PAULHIAC
//	Email: contact@cosmogonies.net
//	WebSite: www.cosmogonies.net
//========================

namespace CG_VisualVariableMonitoring
{
	//Margin layout abilities
	public enum eMarginSide {LeftSide=-1, NoMargin=0, RightSide=1};

	//Curves layout mode
	public enum eLayoutMode {Stacked=1, Overlapped=2};
	
	public class DBG_DataCollector
	{	//Custom class to hold collected data and perform analysis.

		internal System.Reflection.FieldInfo Field;		//The source field
		internal MonoBehaviour Behaviour;				//The source scripted Component
		internal Color VariableColor = Color.white;	//Default Color.

		public List<float> Data;						//The Data !

		public float MaximumValue= float.MinValue;
		public float MinimumValue= float.MaxValue;

		public float Average= 0.0f;

		public DBG_DataCollector(System.Reflection.FieldInfo _Field, MonoBehaviour _Behaviour, Color _Color)
		{	//Constructor
			this.Field = _Field;
			this.Behaviour = _Behaviour;
			this.VariableColor = _Color;
			Data = new List<float>();
		}

		public void addValue(float _NewValue)
		{	//Update with the new Value at current frame.
			this.Data.Add(_NewValue);

			//Update Min/Max
			if( _NewValue > this.MaximumValue ) 
				this.MaximumValue = _NewValue;
			if( _NewValue < this.MinimumValue ) 
				this.MinimumValue = _NewValue;

			//Update Average computation
			float sum=0.0f;
			for(int i=0; i< this.Data.Count; i++)
				sum+=this.Data[i];
			this.Average = sum / this.Data.Count;
		}

		public float getCurrentValue()
		{	//returns the current Value (last one registered).
			if(this.Data.Count==0)//First frame or we just done a clear
				return 0.0f;
			return this.Data[ this.Data.Count-1 ];
		}

		public void clearData()
		{	//wipe all datas, for a total new context.
			this.Data.Clear ();
			this.MaximumValue= 0.0f;
			this.MinimumValue= 0.0f;
			this.Average= 0.0f;
		}

	}

	public class CG_VisualVariableMonitoring  : MonoBehaviour
	{	//The Component that needs to be attached to your playing Camera.

		public eMarginSide MarginSide = eMarginSide.LeftSide;	// Side for the Data's Margin
		public float MarginWidth = 0.15f; 						// in screen ratio

		public eLayoutMode LayoutMode;							// Curve drawing method

		public bool AbsoluteMode =true;						// Does sign matters ?

		public float Opacity = 1.0f;							// The Opacity of the Curves.

		public Dictionary<string, DBG_DataCollector> WatchDict;// All Trackables
		
		public CG_VisualVariableMonitoring()
		{
			this.WatchDict = new Dictionary<string, DBG_DataCollector>();
		}

		void Start()
		{
			//Find tagged public fields:

			//Find what objects to inspect TODO: maybe add a way not to parse everything (layers, tags) ?
			MonoBehaviour[] MonoBehaviourArray =  UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();

			for (int i = 0; i < MonoBehaviourArray.Length; i ++) 
			{
				MonoBehaviour currentBehaviour = MonoBehaviourArray[i];
				//Debug.Log ("Introspecting current class :" +currentBehaviour.name+" of type "+currentBehaviour.GetType().Name);

				System.Reflection.FieldInfo[] FieldArray = currentBehaviour.GetType().GetFields();
				for (int j = 0; j < FieldArray.Length; j ++) 
				{
					System.Reflection.FieldInfo currentField = FieldArray[j];
					object[] CustomAttributeArray = currentField.GetCustomAttributes(true);
					if( CustomAttributeArray.Length>0 )
					{
						for (int k = 0; k < CustomAttributeArray.Length; k ++) 
						{
							if( CustomAttributeArray[k].GetType() == typeof(DBG_Track) )
							{
								//Debug.Log ("\tFound trackable variable @ class :" +currentBehaviour.name+" typeof "+currentBehaviour.GetType().Name +" FieldName = "+ currentField.Name);
								this.WatchDict[currentField.Name] = new DBG_DataCollector(currentField, currentBehaviour,  ((DBG_Track) CustomAttributeArray[k]).VariableColor);
							}
						}
					}
					
				}
			}

		}

		void OnGUI()
		{
			//If No Margin is selected, exiting.
			if(this.MarginSide== eMarginSide.NoMargin)
				return;

			float FontHeight =18.0f;
			float PanelWidth = MarginWidth * Screen.width;

			float XPos=0.0f;
			if(this.MarginSide== eMarginSide.RightSide)
				XPos = Screen.width - (PanelWidth);
			if(this.MarginSide== eMarginSide.LeftSide)
				XPos = 0.0f;

			float SlicedPanelHeight =  (1f/this.WatchDict.Keys.Count) * Screen.height;

			// Displaying values analysis in Margin
			#region ValueAnalysis
			int VariableIteration=1;
			foreach (KeyValuePair<string, DBG_DataCollector> kvp in this.WatchDict)
			{
				int LineIteration=1;
				float YPos = Screen.height - SlicedPanelHeight*VariableIteration;

				DBG_DataCollector current = kvp.Value;

				GUIStyle TextStyle = new GUIStyle();
				TextStyle.normal.textColor = current.VariableColor;

				GUI.Label(new Rect(XPos, YPos+FontHeight*LineIteration, (MarginWidth * Screen.width),FontHeight), "["+current.Field.Name+"]",TextStyle);
				LineIteration++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*LineIteration, (MarginWidth * Screen.width),FontHeight), "[Cur="+current.getCurrentValue().ToString()+"]",TextStyle);
				LineIteration++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*LineIteration, (MarginWidth * Screen.width),FontHeight), "[Min="+current.MinimumValue.ToString()+"]",TextStyle);
				LineIteration++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*LineIteration, (MarginWidth * Screen.width),FontHeight), "[Max="+current.MaximumValue.ToString()+"]",TextStyle);
				LineIteration++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*LineIteration, (MarginWidth * Screen.width),FontHeight), "[Avrg="+current.Average.ToString()+"]",TextStyle);
				LineIteration++;
				LineIteration++;

				VariableIteration++;
			}
			#endregion

			#region Footer
			this.Opacity = GUI.HorizontalSlider (new Rect (XPos, Screen.height - FontHeight * 3, PanelWidth, FontHeight), this.Opacity, 0.0f, 1.0f);
			if( GUI.Button( new Rect(XPos, Screen.height - FontHeight*2, PanelWidth*0.5f,FontHeight), this.LayoutMode.ToString() ) )
			{
				if(this.LayoutMode == eLayoutMode.Stacked)
					this.LayoutMode = eLayoutMode.Overlapped;
				else
					this.LayoutMode = eLayoutMode.Stacked;
			}
			if( GUI.Button( new Rect(XPos+ PanelWidth*0.5f, Screen.height - FontHeight*2, PanelWidth*0.5f,FontHeight), "Abs" ) )
			{
				this.AbsoluteMode = !this.AbsoluteMode;
			}
			if( GUI.Button( new Rect(XPos, Screen.height - FontHeight, PanelWidth,FontHeight), "Clear" ) )
			{
				foreach (KeyValuePair<string, DBG_DataCollector> kvp in this.WatchDict)
				{
					kvp.Value.clearData();
				}
			}
			#endregion

		}


		void LateUpdate()
		{
			//Adding current Values into the WatchDict.
			foreach (KeyValuePair<string, DBG_DataCollector> kvp in this.WatchDict) 
			{
				float currentValue = (float) kvp.Value.Field.GetValue (kvp.Value.Behaviour); //TODO: be sure the cast is possible

				if(this.AbsoluteMode)
					currentValue = Mathf.Abs (currentValue);

				kvp.Value.addValue(currentValue);
			}
			//Drawing the curves :
			if (this.Opacity > 0.0f) 
			{
				int i = 0;
				foreach (KeyValuePair<string, DBG_DataCollector> kvp in this.WatchDict) 
				{
					DrawCurve (kvp.Value, i, this.WatchDict.Keys.Count, this.Opacity);
					i++;
				}
			}
		}






		void DrawCurve(DBG_DataCollector _DataCollector, int _SliceIteration, int _SliceCount, float _Opacity)
		{	//Draw the curve for the given DataCollector
			float XPosAsRatio = 0.0f;
			float value = 0.0f;
			
			List< Vector3 > ViewPortBuffer = new List<Vector3>();

			Color TheColor = _DataCollector.VariableColor;
			if(_Opacity<1.0f)
				TheColor = new Color(TheColor.r, TheColor.g, TheColor.b, _Opacity);

			for(int i=0; i< _DataCollector.Data.Count; i++)
			{
				value = _DataCollector.Data[i];

				//Determining the XPos as a ratio for the screen
				if(this.MarginSide == eMarginSide.RightSide)
					XPosAsRatio =  Mathf.Lerp(0.0f,1.0f-MarginWidth, (float) i / (_DataCollector.Data.Count -1) ) ;
				else if(this.MarginSide == eMarginSide.LeftSide)
					XPosAsRatio =  Mathf.Lerp(MarginWidth,1.0f, (float) i / (_DataCollector.Data.Count -1) ) ;
				else
					XPosAsRatio =  Mathf.Lerp(0.0f,1.0f, (float) i / (_DataCollector.Data.Count -1) ) ;
				
				//Determining the YPos as a ratio for the screen, according to Stacked layout and AbsoluteMode:
				float YPosAsRatio = 0.0f;
				if( this.AbsoluteMode)
				{
					if(_DataCollector.MaximumValue!=0.0f)
						YPosAsRatio = value/_DataCollector.MaximumValue;
				}
				else
				{
					float Max=1.0f;

					if( Mathf.Abs(_DataCollector.MinimumValue) > _DataCollector.MaximumValue )
					{
						if(_DataCollector.MinimumValue!=0.0f)
							Max = Mathf.Abs(_DataCollector.MinimumValue);
					}
					else
					{
						if(_DataCollector.MaximumValue!=0.0f)
							Max = _DataCollector.MaximumValue;
					}

					YPosAsRatio = Mathf.Abs(value) / Max;

					if(value>=0.0f)
						YPosAsRatio = 0.5f+(YPosAsRatio*0.5f);
					else
						YPosAsRatio = 0.5f-(YPosAsRatio*0.5f);
				}

				if( this.LayoutMode == eLayoutMode.Stacked)
				{
						YPosAsRatio = (1f/_SliceCount)*_SliceIteration + (YPosAsRatio*(1f/_SliceCount)  );
				}

				Vector3 Point2D_Value = new Vector3(XPosAsRatio, YPosAsRatio, Camera.main.nearClipPlane*2  );

				ViewPortBuffer.Add(Point2D_Value);

				if(i>0)
				{
					Vector3 Point3D_ValuePrevious = Camera.main.ViewportToWorldPoint( ViewPortBuffer[i-1] );
					Vector3 Point3D_Value = Camera.main.ViewportToWorldPoint(Point2D_Value);

					Debug.DrawLine(Point3D_ValuePrevious, Point3D_Value, TheColor, 0f);	//duration à 0f(seconds) => 1 frame
				}
			}
		}

		/*
		void DrawColumns()
		{

			float ratio = 0.0f;
			float value = 0.0f;

			//float ratioPrevious = 0.0f;
			//float valuePrevious = 0.0f;

			Vector3 Point2D_ValuePrevious;

			for(int i=0; i< this.DataValue.Count; i++)
			{
				//ratioPrevious = ratio;
				//valuePrevious = value;

				ratio =  Mathf.Lerp(MarginWidth,1.0f-MarginWidth, (float) i / (this.DataValue.Count -1) ) ;

				value = this.DataValue[i];
				float valueRatio = value/this.MaximumValue;

				Vector3 Point2D_Base = new Vector3(ratio, 0.0f, Camera.main.nearClipPlane*2  );
				Vector3 Point2D_Value = new Vector3(ratio, valueRatio, Camera.main.nearClipPlane*2  );

				Vector3 Point3D_Base = Camera.main.ViewportToWorldPoint(Point2D_Base);
				Vector3 Point3D_Value = Camera.main.ViewportToWorldPoint(Point2D_Value);

				Debug.DrawLine(Point3D_Base, Point3D_Value, Color.yellow,0f);	//duration à 0f(seconds) => 1 frame

			}
		}
		*/

	}
}







