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

		public static List<Dictionary<string, dynamic>> getThread (string url, string sort = "top", string limit = "200") 
		{
			JavaScriptSerializer jss = new JavaScriptSerializer ();
			return jss.Deserialize<List<Dictionary<string, dynamic>>> (getRequest (url + "?sort=" + sort + "&limit=" + limit));
		}

		public static void printComments (/*List<Dictionary<string, dynamic>>*/dynamic r, int curDepth = 0, int maxDepth = -1) 
		{
			foreach (Dictionary<string, dynamic> c in r) {
				try {
				string text = indent(c ["data"] ["author"] + " | " + c ["data"] ["score"] + " karma" + "\n" +
					c ["data"] ["body"], curDepth);
				Console.WriteLine (text);
				//if (c ["data"] ["replies"] ["data"] ["children"].Count > 0 && c ["data"] ["replies"] ["data"] ["children"][0].GetType() != typeof(string))
					printComments (c ["data"] ["replies"] ["data"] ["children"], curDepth + 1, maxDepth);
				} catch {
				}
			}
		}

		public static void printThread(List<Dictionary<string, dynamic>> r)
		{
			Dictionary<string, dynamic> body = r [0] ["data"] ["children"] [0] ["data"];
			Console.WriteLine (body["title"] + " - by " + body["author"]);
			//Console.WriteLine (body ["selftext"]);
			Console.WriteLine(body["url"]);
			Console.WriteLine (indent(body ["selftext"]));
			Console.WriteLine (body ["num_comments"].ToString () + " comments.");
			printComments (r [1]["data"]["children"]);
		}

		public static void printSub(dynamic r, int selected = 0)
		{
			Console.Clear ();
			foreach (int t in Enumerable.Range(0, r.Count)) {
				string toWrite = r [t] ["data"] ["title"] + " - by " + r [t] ["data"] ["author"];
				if (t == selected) {
					ConsoleColor prevFore = Console.ForegroundColor;
					ConsoleColor prevBack = Console.BackgroundColor;
					Console.ForegroundColor = (ConsoleColor)Math.Abs ((int)prevFore - 7);
					Console.BackgroundColor = (ConsoleColor)Math.Abs ((int)prevBack - 7);
					Console.WriteLine (toWrite);
					Console.ForegroundColor = prevFore;
					Console.BackgroundColor = prevBack;
				} else {
					Console.WriteLine (toWrite);
				}
			}
		}

		public static void selectThread (string sub, string sort = "top", string limit = "25")
		{
			Dictionary<string, dynamic> r = getSub (Constants.SUB + sub + ".json" + "?sort=" + sort + "&limit=" + limit);
			/*List<Dictionary<string, dynamic>>*/dynamic threads = r ["data"] ["children"];
			printSub (threads);

			int selected = 0;
			ConsoleKey k = ConsoleKey.Spacebar;
			while (k != ConsoleKey.Enter) {
				k = Console.ReadKey ().Key;
				if (k == ConsoleKey.DownArrow) {
					selected = Math.Min (selected + 1, threads.Count - 1);
					printSub (threads, selected);
				} else if (k == ConsoleKey.UpArrow) {
					selected = Math.Max (selected - 1, 0);
					printSub (threads, selected);
				}
			}
			Console.Clear ();
			printThread(getThread(Constants.SUB + sub + "/comments/" +
				threads[selected]["data"]["id"] + ".json"));
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
				while (s != "" && s != "." && s != ":" && s != ")") {
					//Console.WriteLine (s); //DEBUG
					int index = (s.Length <= length) ?
						s.Length - 1 : Math.Max (
						            Math.Max (
							            Math.Max (
								            s.LastIndexOf (' ', length - 1),
								            s.LastIndexOf ('.', length - 1)),
							            s.LastIndexOf (':', length - 1)),
						            s.LastIndexOf (')', length - 1));
					if (index == -1)
						index = length - 1;
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
			selectThread (sub);
			//List<Dictionary<string, dynamic>> r = getThread (Constants.SUB + sub + ".json");
			//printThread (r);
		}
	}
}
