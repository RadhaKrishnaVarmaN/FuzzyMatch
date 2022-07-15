using System;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

public partial class SqlUserFunctions
{
    static readonly Regex punctuations = new Regex(@"[ ()\[\]!'\\"":;,?./\-_]");
    static readonly Single equalMatch = 1.0F;

    [SqlFunction]
    public static SqlSingle FuzzyQGram(string source, string target)
    {
        Single result = 0.0F;

        source = (source == null) ? "" : source.Trim().ToUpper();
        target = (target == null) ? "" : target.Trim().ToUpper();

        if (source == target)
        {
            return equalMatch;
        }

        source = punctuations.Replace(source, "");
        target = punctuations.Replace(target, "");

        if (source == target)
        {
            return equalMatch;
        }

        if (source == "" || target == "")
        {
            return result;
        }

        int qSize = 2;
        int count = 0;

        if (source.Length >= qSize || target.Length >= qSize)
        {
            //QGram Algorithm
            for (int i = 0; i < (source.Length + 1 - qSize); i++)
            {
                count += target.Contains(source.Substring(i, qSize)) ? 1 : 0;
            }

            //Q is size of gram (two letters for example)
            //Result = (2 * Matched Q-Grams)/ (Number of Q-grams in 1 + number of Qgrams in 2)
            result = ((Single)(count * 2)) / ((source.Length + 1 - qSize) + (target.Length + 1 - qSize));
        }

        return result;
    }

    [SqlFunction]
    public static SqlSingle FuzzyLCS(string source, string target)
    {
        Single result = 0.0F;

        source = (source == null) ? "" : source.Trim().ToUpper();
        target = (target == null) ? "" : target.Trim().ToUpper();

        if (source == target)
        {
            return equalMatch;
        }

        source = punctuations.Replace(source, "");
        target = punctuations.Replace(target, "");

        if (source == target)
        {
            return equalMatch;
        }

        if (source == "" || target == "")
        {
            return result;
        }

        int[] curRow = new int[target.Length + 1];
        char[] tChars = target.ToCharArray();

        int preRowPrePosValue = 0;
        int preRowCurPosValue = 0;
        int curRowPrePosValue = 0;
        int curRowCurPosValue = 0;

        // Longest Common Subsequence Algorithm
        foreach (char sChar in source.ToCharArray())
        {
            preRowPrePosValue = 0;
            curRowPrePosValue = 0;
            count = 0;

            foreach (char tChar in tChars)
            {
                count++;

                preRowCurPosValue = curRow[count];
                curRowCurPosValue = (sChar == tChar) ? (1 + preRowPrePosValue) : Math.Max(curRowPrePosValue, preRowCurPosValue);

                if (preRowCurPosValue != curRowCurPosValue)
                {
                    curRow[count] = curRowCurPosValue;
                }

                preRowPrePosValue = preRowCurPosValue;
                curRowPrePosValue = curRowCurPosValue;
            }
        }

        //Compare String 1 with String 2 using from 1 to the total number of characters in string 1 as an instring comparison.  
        //Return the greatest number of characters in the string that match, divide this by [(length of string 1 + length of string 2)/2]
        result += ((Single)(curRowCurPosValue * 2) / (source.Length + target.Length));

        return result;
    }
}
