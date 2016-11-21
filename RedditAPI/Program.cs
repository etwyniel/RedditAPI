using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace RedditAPI
{
	class Constants {
		public const string BASE = "https://reddit.com/";
		public const string SUB = BASE + "r/";
		public const string USER = BASE + "u/";
	}


	class MainClass
	{

		public static string getRequest(string url)
		{
			string text;

			HttpWebRequest r = (HttpWebRequest)WebRequest.Create (url);
			r.Method = WebRequestMethods.Http.Get;
			r.Accept = "application/json";
			WebResponse resp = r.GetResponse ();
			using (var sr = new StreamReader (resp.GetResponseStream ())) {
				text = sr.ReadToEnd ();
			}
			return text;
		}

		public static Dictionary<string,dynamic> getSub(string url) 
		{
			JavaScriptSerializer jss = new JavaScriptSerializer ();
			return jss.Deserialize<Dictionary<string, dynamic>> (getRequest (url));
		}

		public static List<Dictionary<string, dynamic>> getThread (string url) 
		{
			JavaScriptSerializer jss = new JavaScriptSerializer ();
			return jss.Deserialize<List<Dictionary<string, dynamic>>> (getRequest (url));
		}

		public static void printComments (/*List<Dictionary<string, dynamic>>*/dynamic r, int curDepth = 0, int maxDepth = -1) 
		{
			foreach (Dictionary<string, dynamic> c in r) {
				string text = indent(c ["data"] ["author"] + " | " + c ["data"] ["score"] + " karma" + "\n" +
					c ["data"] ["body"], curDepth);
				Console.WriteLine (text);
				if (c ["data"] ["replies"] ["data"] ["children"].Count > 0 && c ["data"] ["replies"] ["data"] ["children"][0].GetType() != typeof(string))
				printComments (c ["data"] ["replies"] ["data"] ["children"], curDepth + 1, maxDepth);
			}
		}

		public static void printThread(List<Dictionary<string, dynamic>> r)
		{
			Dictionary<string, dynamic> body = r [0] ["data"] ["children"] [0] ["data"];
			Console.WriteLine (body["title"] + " - by " + body["author"]);
			Console.WriteLine (indent(body ["selftext"]));
			Console.WriteLine (body ["num_comments"].ToString () + " comments.");
			printComments (r [1]["data"]["children"]);
		}

		public static string indent (string s, int n = 0, int baseIndent = 4) {
			string r = "";
			if (s.Contains ('\n')) {
				foreach (string sub in s.Split ('\n')) {
					r += indent (sub, n, baseIndent);
				}
				return r;
			} else {
				int length = Console.BufferWidth - n * baseIndent;
				string ind = new String (' ', baseIndent * n);
				while (s != "" && s != ".") {
					int index = (s.Length <= length) ?
						s.Length - 1 : Math.Max(s.LastIndexOf (' ', length - 1), s.LastIndexOf('.', length - 1));
					r += ind + s.Substring (0, index + 1) + "\n";
					s = s.Remove (0, index + 1);
				}
				return r;
			}
		}

		public static void Main (string[] args)
		{
			Console.Write ("Choose a subreddit: ");
			string sub = Console.ReadLine ();
			List<Dictionary<string, dynamic>> r = getThread (Constants.SUB + sub + ".json");
			printThread (r);
		}
	}
}
