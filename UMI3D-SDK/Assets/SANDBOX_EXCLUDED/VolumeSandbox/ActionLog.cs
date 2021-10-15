using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;


public class ActionLog : umi3d.common.Singleton<ActionLog>
{
    protected static Dictionary<string, ActionLog> Singletons = new Dictionary<string, ActionLog>();
    protected static string Separator = ",";
    public static bool Logging = false;

    protected string id;

    protected string path;
    protected string directoryName;

    protected List<string> waitList = new List<string>();
    protected bool waitCoroutineLaunched = false;

    #region Static Functions

    public static ActionLog GetActionLog(string id)
    {
        if (!Singletons.ContainsKey(id))
            Singletons.Add(id, new ActionLog(id));

        return Singletons[id];
    }

    /// <summary>
    /// Need data formated for UTF-8.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="separator"></param>
    /// <param name="datas"></param>
    public static void Log(string id, params string[] datas)
    {
        if (Logging)
            GetActionLog(id).Log(datas);
    }

    protected static DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    public static double ConvertToUnixTimestamp(DateTime date)
    {
        return Math.Floor((date.ToUniversalTime() - origin).TotalMilliseconds);
    }

    public static string PrepareString(string str, bool replaceLineBreak, bool replaceQuote)
    {
        if (replaceLineBreak)
            str = str.Replace("\n", "\\n");

        if (replaceQuote)
            str = str.Replace("\"", "\\\"");

        return "\"" + str + "\"";
    }

    public static string PrepareFloat(float f)
    {
        return f.ToString().Replace(',', '.');
    }

    public static string PrepareDouble(double d)
    {
        return d.ToString().Replace(',', '.');
    }

    public static string PrepareBool(bool b)
    {
        return b ? "True" : "False";
    }

    #endregion

    #region Class Functions

    public ActionLog(string id)
    {
        this.id = id;

        string appRoot = System.IO.Path.GetFullPath(UnityEngine.Application.dataPath + "/../");

        path = inetum.unityUtils.Path.Combine(appRoot, "MyLogs/", id + ".log");
        directoryName = System.IO.Path.GetDirectoryName(path);
    }

    /// <summary>
    /// Log datas to the logging file.
    /// </summary>
    public void Log(params string[] datas)
    {
        if (!Logging)
            return;

        if (!Directory.Exists(directoryName))
            Directory.CreateDirectory(directoryName);

        string str = string.Join(Separator, datas) + "\n";

        try
        {
            File.AppendAllText(path, str);
        }
        catch
        {
            waitList.Add(str);
            if (!waitCoroutineLaunched)
                Instance.StartCoroutine(WaitForFileToOpen());
        }
    }

    protected IEnumerator WaitForFileToOpen()
    {
        waitCoroutineLaunched = true;

        while (waitList.Count > 0)
        {
            yield return new WaitForSecondsRealtime(1f);

            try
            {
                foreach (string str in waitList)
                {
                    File.AppendAllText(path, str);
                }

                waitList.Clear();
            }
            catch
            {

            }
        }

        waitCoroutineLaunched = false;
    }

    #endregion
}
