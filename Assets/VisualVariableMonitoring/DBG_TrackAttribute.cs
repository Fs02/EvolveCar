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

//Custom Attribute to tag a public field for visual debugging
public class DBG_Track : System.Attribute
{
	public Color VariableColor;
	

	public DBG_Track(string _ColorName)
	{	//Constructor for a given color as string
		this.VariableColor = this.getColorByName(_ColorName);
	}	
	
	public DBG_Track(float _Red, float _Green, float _Blue)
	{ //Constructor for a given color as R,G,B

		//Unity uses ratio for color channels :
		//maybe user still use 255 range
		if (_Red > 1.0f)
			_Red /= 255;
		if (_Green > 1.0f)
			_Green /= 255;
		if (_Blue > 1.0f)
			_Blue /= 255;
		
		this.VariableColor = new Color (_Red, _Green,_Blue);
	}

	public DBG_Track()
	{
		//No Color determined, random time:
		this.VariableColor = new Color( UnityEngine.Random.value, UnityEngine.Random.value,UnityEngine.Random.value);
	}
	//public DBG_Track(Color _Color)

	public Color getColorByName(string _ColorName)
	{
		//Looking for a unity's Color with that name:
		PropertyInfo[] pi = typeof(Color).GetProperties( BindingFlags.Public | BindingFlags.Static | BindingFlags.SetProperty);
		foreach (PropertyInfo info in pi)
		{
			if( info.Name.ToUpper() == _ColorName.ToUpper() )	//avoiding case errors
			{
				return (Color) info.GetValue(typeof(Color), null);
			}
		}
		
		// If nothing is found, let's try w3c names:
		//According to the w3c http://www.w3schools.com/html/html_colorvalues.asp 
		Dictionary<string,string> ColorDict = new Dictionary<string, string>();	// ColorName=> Hexvalue
		ColorDict["Black"]=	"#000000";
		ColorDict["Navy"]=	"#000080";
		ColorDict["DarkBlue"]=	"#00008B";
		ColorDict["MediumBlue"]=	"#0000CD";
		ColorDict["Blue"]=	"#0000FF";
		ColorDict["DarkGreen"]=	"#006400";
		ColorDict["Green"]=	"#008000";
		ColorDict["Teal"]=	"#008080";
		ColorDict["DarkCyan"]=	"#008B8B";
		ColorDict["DeepSkyBlue"]=	"#00BFFF";
		ColorDict["DarkTurquoise"]=	"#00CED1";
		ColorDict["MediumSpringGreen"]=	"#00FA9A";
		ColorDict["Lime"]=	"#00FF00";
		ColorDict["SpringGreen"]=	"#00FF7F";
		ColorDict["Aqua"]=	"#00FFFF";
		ColorDict["Cyan"]=	"#00FFFF";
		ColorDict["MidnightBlue"]=	"#191970";
		ColorDict["DodgerBlue"]=	"#1E90FF";
		ColorDict["LightSeaGreen"]=	"#20B2AA";
		ColorDict["ForestGreen"]=	"#228B22";
		ColorDict["SeaGreen"]=	"#2E8B57";
		ColorDict["DarkSlateGray"]=	"#2F4F4F";
		ColorDict["LimeGreen"]=	"#32CD32";
		ColorDict["MediumSeaGreen"]=	"#3CB371";
		ColorDict["Turquoise"]=	"#40E0D0";
		ColorDict["RoyalBlue"]=	"#4169E1";
		ColorDict["SteelBlue"]=	"#4682B4";
		ColorDict["DarkSlateBlue"]=	"#483D8B";
		ColorDict["MediumTurquoise"]=	"#48D1CC";
		ColorDict["Indigo"]= 	"#4B0082";
		ColorDict["DarkOliveGreen"]=	"#556B2F";
		ColorDict["CadetBlue"]=	"#5F9EA0";
		ColorDict["CornflowerBlue"]=	"#6495ED";
		ColorDict["MediumAquaMarine"]=	"#66CDAA";
		ColorDict["DimGray"]=	"#696969";
		ColorDict["SlateBlue"]=	"#6A5ACD";
		ColorDict["OliveDrab"]=	"#6B8E23";
		ColorDict["SlateGray"]=	"#708090";
		ColorDict["LightSlateGray"]=	"#778899";
		ColorDict["MediumSlateBlue"]=	"#7B68EE";
		ColorDict["LawnGreen"]=	"#7CFC00";
		ColorDict["Chartreuse"]=	"#7FFF00";
		ColorDict["Aquamarine"]=	"#7FFFD4";
		ColorDict["Maroon"]=	"#800000";
		ColorDict["Purple"]=	"#800080";
		ColorDict["Olive"]=	"#808000";
		ColorDict["Gray"]=	"#808080";
		ColorDict["SkyBlue"]=	"#87CEEB";
		ColorDict["LightSkyBlue"]=	"#87CEFA";
		ColorDict["BlueViolet"]=	"#8A2BE2";
		ColorDict["DarkRed"]=	"#8B0000";
		ColorDict["DarkMagenta"]=	"#8B008B";
		ColorDict["SaddleBrown"]=	"#8B4513";
		ColorDict["DarkSeaGreen"]=	"#8FBC8F";
		ColorDict["LightGreen"]=	"#90EE90";
		ColorDict["MediumPurple"]=	"#9370DB";
		ColorDict["DarkViolet"]=	"#9400D3";
		ColorDict["PaleGreen"]=	"#98FB98";
		ColorDict["DarkOrchid"]=	"#9932CC";
		ColorDict["YellowGreen"]=	"#9ACD32";
		ColorDict["Sienna"]=	"#A0522D";
		ColorDict["Brown"]=	"#A52A2A";
		ColorDict["DarkGray"]=	"#A9A9A9";
		ColorDict["LightBlue"]=	"#ADD8E6";
		ColorDict["GreenYellow"]=	"#ADFF2F";
		ColorDict["PaleTurquoise"]=	"#AFEEEE";
		ColorDict["LightSteelBlue"]=	"#B0C4DE";
		ColorDict["PowderBlue"]=	"#B0E0E6";
		ColorDict["FireBrick"]=	"#B22222";
		ColorDict["DarkGoldenRod"]=	"#B8860B";
		ColorDict["MediumOrchid"]=	"#BA55D3";
		ColorDict["RosyBrown"]=	"#BC8F8F";
		ColorDict["DarkKhaki"]=	"#BDB76B";
		ColorDict["Silver"]=	"#C0C0C0";
		ColorDict["MediumVioletRed"]=	"#C71585";
		ColorDict["IndianRed"]= 	"#CD5C5C";
		ColorDict["Peru"]=	"#CD853F";
		ColorDict["Chocolate"]=	"#D2691E";
		ColorDict["Tan"]=	"#D2B48C";
		ColorDict["LightGray"]=	"#D3D3D3";
		ColorDict["Thistle"]=	"#D8BFD8";
		ColorDict["Orchid"]=	"#DA70D6";
		ColorDict["GoldenRod"]=	"#DAA520";
		ColorDict["PaleVioletRed"]=	"#DB7093";
		ColorDict["Crimson"]=	"#DC143C";
		ColorDict["Gainsboro"]=	"#DCDCDC";
		ColorDict["Plum"]=	"#DDA0DD";
		ColorDict["BurlyWood"]=	"#DEB887";
		ColorDict["LightCyan"]=	"#E0FFFF";
		ColorDict["Lavender"]=	"#E6E6FA";
		ColorDict["DarkSalmon"]=	"#E9967A";
		ColorDict["Violet"]=	"#EE82EE";
		ColorDict["PaleGoldenRod"]=	"#EEE8AA";
		ColorDict["LightCoral"]=	"#F08080";
		ColorDict["Khaki"]=	"#F0E68C";
		ColorDict["AliceBlue"]=	"#F0F8FF";
		ColorDict["HoneyDew"]=	"#F0FFF0";
		ColorDict["Azure"]=	"#F0FFFF";
		ColorDict["SandyBrown"]=	"#F4A460";
		ColorDict["Wheat"]=	"#F5DEB3";
		ColorDict["Beige"]=	"#F5F5DC";
		ColorDict["WhiteSmoke"]=	"#F5F5F5";
		ColorDict["MintCream"]=	"#F5FFFA";
		ColorDict["GhostWhite"]=	"#F8F8FF";
		ColorDict["Salmon"]=	"#FA8072";
		ColorDict["AntiqueWhite"]=	"#FAEBD7";
		ColorDict["Linen"]=	"#FAF0E6";
		ColorDict["LightGoldenRodYellow"]=	"#FAFAD2";
		ColorDict["OldLace"]=	"#FDF5E6";
		ColorDict["Red"]=	"#FF0000";
		ColorDict["Fuchsia"]=	"#FF00FF";
		ColorDict["Magenta"]=	"#FF00FF";
		ColorDict["DeepPink"]=	"#FF1493";
		ColorDict["OrangeRed"]=	"#FF4500";
		ColorDict["Tomato"]=	"#FF6347";
		ColorDict["HotPink"]=	"#FF69B4";
		ColorDict["Coral"]=	"#FF7F50";
		ColorDict["DarkOrange"]=	"#FF8C00";
		ColorDict["LightSalmon"]=	"#FFA07A";
		ColorDict["Orange"]=	"#FFA500";
		ColorDict["LightPink"]=	"#FFB6C1";
		ColorDict["Pink"]=	"#FFC0CB";
		ColorDict["Gold"]=	"#FFD700";
		ColorDict["PeachPuff"]=	"#FFDAB9";
		ColorDict["NavajoWhite"]=	"#FFDEAD";
		ColorDict["Moccasin"]=	"#FFE4B5";
		ColorDict["Bisque"]=	"#FFE4C4";
		ColorDict["MistyRose"]=	"#FFE4E1";
		ColorDict["BlanchedAlmond"]=	"#FFEBCD";
		ColorDict["PapayaWhip"]=	"#FFEFD5";
		ColorDict["LavenderBlush"]=	"#FFF0F5";
		ColorDict["SeaShell"]=	"#FFF5EE";
		ColorDict["Cornsilk"]=	"#FFF8DC";
		ColorDict["LemonChiffon"]=	"#FFFACD";
		ColorDict["FloralWhite"]=	"#FFFAF0";
		ColorDict["Snow"]=	"#FFFAFA";
		ColorDict["Yellow"]=	"#FFFF00";
		ColorDict["LightYellow"]=	"#FFFFE0";
		ColorDict["Ivory"]=	"#FFFFF0";
		ColorDict["White"]=	"#FFFFFF";
		
		foreach (KeyValuePair<string,string> kvp in ColorDict)
		{
			if( kvp.Key.ToUpper() == _ColorName.ToUpper() )
			{
				string HexRed = kvp.Value.Substring(1,2);
				string HesGreen = kvp.Value.Substring(3,2);
				string HexBlue = kvp.Value.Substring(5,2);
				
				int red = Convert.ToInt32(HexRed, 16);
				int green = Convert.ToInt32(HesGreen, 16);
				int blue = Convert.ToInt32(HexBlue, 16);
				return new Color(red/255.0f, green/255.0f,blue/255.0f);
			}
			
		}
		return Color.black;		//Default Color if no matching.
	}
}

