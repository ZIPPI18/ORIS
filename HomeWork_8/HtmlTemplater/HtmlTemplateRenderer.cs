using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace HtmlTemplater
{
    public class HtmlTemplateRenderer : IHtmlTemplateRenderer
    {
        public string RenderFromString(string htmlTemplate, object dataModel)
        {
            if (htmlTemplate == null) return string.Empty;
            if (dataModel == null) return htmlTemplate;

            Type myType = dataModel.GetType();

            Regex regex = new Regex(@"\$\{\s*[\w\d_]+\s*\.\s*([\w\d_]+)\s*\}");

            string result = regex.Replace(htmlTemplate, (match) =>
            {
                string propertyName = match.Groups[1].Value.Trim();

                PropertyInfo property = myType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (property != null)
                {
                    object value = property.GetValue(dataModel);
                    return value?.ToString() ?? string.Empty;
                }

                return match.Value;
            });

            return result;
        }

        public string RenderFromFile(string filePath, object dataModel)
        {
            throw new NotImplementedException();
        }

        public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
        {
            throw new NotImplementedException();
        }
    }
}
