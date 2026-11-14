using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HtmlTemplater
{
    public class TemplateParser
    {
        public static string ProcessIfConditions (string htmlTemplate, object dataModel)
        {
            if (string.IsNullOrEmpty(htmlTemplate) || dataModel == null)
                return htmlTemplate;

            Regex ifRegex = new Regex(@"\$if\(\s*[\w\d_]+\s*\.\s*([\w\d_]+)\s*\)(.*?)\$endif");

            string result = ifRegex.Replace(htmlTemplate, (match) => 
            {
                string propertyName = match.Groups[1].Value.Trim();

                string value = match.Groups[2].Value;

                Type myType = dataModel.GetType();

                PropertyInfo propertyInfo = myType.GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.IgnoreCase);

                bool conditionResult = false;

                if (propertyInfo != null )
                {
                    object actualValue = propertyInfo.GetValue(dataModel);

                    if(actualValue is bool)
                    {
                        bool result = (bool)actualValue;
                        conditionResult = result;
                    }
                }

                string[] parts = value.Split("$else");

                string ifTrueContent = parts[0];
                string ifFalseContent = string.Empty;

                if (parts.Length > 1)
                {
                    ifFalseContent = parts[1];
                }
                
                if(conditionResult == true)
                {
                    return ifTrueContent;
                }
                else
                {
                    return ifFalseContent;
                }
            });

            return result;
        }

        public static string ProcessForeach(string htmlTemplate, object dataModel)
        {
            if (string.IsNullOrEmpty(htmlTemplate) || dataModel == null)
                return htmlTemplate;

            Regex foreachRegex = new Regex(@"\$foreach\(\s*[\w\d_]+\s*\)(.*?)\$endforeach", RegexOptions.Singleline);

            return foreachRegex.Replace(htmlTemplate, (match) =>
            {
                string innerContent = match.Groups[1].Value;

                if (!(dataModel is System.Collections.IEnumerable collection))
                    return string.Empty;

                StringBuilder loopResult = new StringBuilder();

                foreach (object item in collection)
                {
                    if (item == null) continue;

                    string itemContent = innerContent;
                    Regex itemPropertyRegex = new Regex(@"\$item\.\s*([\w\d_]+)");

                    string renderedItem = itemPropertyRegex.Replace(itemContent, (itemMatch) =>
                    {
                        string itemPropName = itemMatch.Groups[1].Value.Trim();

                        Type itemType = item.GetType();
                        PropertyInfo itemPropInfo = itemType.GetProperty(itemPropName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

                        return itemPropInfo != null ? itemPropInfo.GetValue(item)?.ToString() : itemMatch.Value;
                    });

                    loopResult.Append(renderedItem + "\n");
                }

                return loopResult.ToString();
            });
        }
    }
}
