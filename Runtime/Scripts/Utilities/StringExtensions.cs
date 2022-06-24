using UnityEngine;
using System.Globalization;
using System;

public static class StringExtensions
{
    static NumberFormatInfo englishLocaleNumberFormat = new CultureInfo("en-US").NumberFormat;
    public static Vector3 ParseVector3(string vec, string from = "", float _default = 0f)
    {
        Debug.LogFormat("trying to parse vector: {0} from {1}", vec, from);
        var splits = vec.Split(',');
        var vector = new Vector3(0, 0, 0);

        vector.x = ParseFloat(splits[0].TrimStart('('), string.Format("Failed to parse X value of vector {0} from {1}", vec, from), _default);

        vector.y = ParseFloat(splits[1], string.Format("Failed to parse Y value of vector {0} from {1}", vec, from), _default);

        vector.z = ParseFloat(splits[2].TrimEnd(')'), string.Format("Failed to parse Z value of vector {0} from {1}", vec, from), _default);
        return vector;
    }
    public static bool TryParseVector3(string vec, out Vector3 value)
    {
        var splits = vec.Split(',');
        if (splits.Length != 3)
        {
            value = new Vector3(0,0,0);
            return false;
        }
        var x = splits[0].TrimStart('(');
        var y = splits[1];
        var z = splits[2].TrimEnd(')');
        float x_,y_,z_;
        if (float.TryParse(x, out x_) && float.TryParse(y, out y_) && float.TryParse(z,out z_))
        {
            value = new Vector3(x_,y_,z_);
            return true;
        }
        else
        {
            value = new Vector3(0,0,0);
            return false;
        }
    }
    public static Vector2 ParseVector2(string vec, string from = "", float _default = 0f)
    {
        Debug.LogFormat("trying to parse vector: {0} from {1}", vec, from);
        var splits = vec.Split(',');
        var vector = new Vector2(0, 0);

        vector.x = ParseFloat(splits[0].TrimStart('('), string.Format("Failed to parse X value of vector {0} from {1}", vec, from), _default);


        vector.y = ParseFloat(splits[1].TrimEnd(')'), string.Format("Failed to parse Y value of vector {0} from {1}", vec, from), _default);
        return vector;
    }
    public static float ParseFloat(string fl, string from = "", float _default = 0f)
    {
        bool result;
        float f = _default;
        result = float.TryParse(fl.TrimStart(null).TrimEnd(null), NumberStyles.Float, englishLocaleNumberFormat, out f);
        if (!result)
        {
            Debug.LogErrorFormat("Failed to parse float {0} from function {1}", fl, from);
            
        }
        return f;
    }

     public static bool IsAllUpper(string input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                return false;
        }
        return true;
    }

    

}