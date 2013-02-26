using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader
{
    class HashQueryParser
    {
        public static Dictionary<string, string> Parse(string input)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            int hashPos = input.IndexOf('#');
            if (hashPos < 0)
                return parameters;

            string queryOnly = input.Substring(hashPos + 1);

            string[] parametersArr = queryOnly.Split('&');

            char[] paramSplitChars = new char[] {'='};
            foreach (string p in parametersArr)
            {
                string[] keyvalue = p.Split(paramSplitChars, 2);
                parameters.Add(keyvalue[0], keyvalue[1]);
            }

            return parameters;
        }
    }
}
