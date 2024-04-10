﻿using System;
using System.Diagnostics;
using System.IO;

namespace TCC.Utils;

public static class Log
{
    public static event Action<ChatChannel, string, string>? NewChatMessage;
    public static event Func<string, string, NotificationType, int, NotificationTemplate, int, int>? NewNotification;
    public static event Action<string>? NewMessageBox;

    private static string _logPath = "logs";
    private static string _version = "";

    public static void Config(string path, string version)
    {
        _logPath = path;
        _version = version;
    }

    /// <summary>
    /// Logs a message to Debug.WriteLine().
    /// </summary>
    /// <param name="line"></param>
    public static void CW(string line)
    {
        Debug.WriteLine(line);
        //if (Debugger.IsAttached) Debug.WriteLine(line);
        //else
        //{
        //Console.WriteLine(line);
        //}
    }
    /// <summary>
    /// Logs a message to file.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="fileName"></param>
    public static void F(string line, string fileName = "error.log")
    {
        try
        {
            if (!Directory.Exists(_logPath))
                Directory.CreateDirectory(_logPath);
            File.AppendAllText(Path.Combine(_logPath, fileName), $"############### {_version} - {DateTime.Now:dd/MM/yyyy HH:mm:ss} ###############\n{line}\n\n");
        }
        catch
        {
            // ignored
        }
    }
    /// <summary>
    /// Fires a new notification, returning the id of the returned notification.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="line"></param>
    /// <param name="type"></param>
    /// <param name="duration"></param>
    /// <param name="template"></param>
    /// <param name="forcedId"></param>
    /// <returns></returns>
    public static int N(string title, string line, NotificationType type, int duration = -1, NotificationTemplate template = NotificationTemplate.Default, int forcedId = -1)
    {
        return NewNotification?.Invoke(title, line, type, duration, template, forcedId) ?? -1;
    }
    /// <summary>
    /// Logs a message to chat. &lt;font/&gt; tags are added if missing.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="author"></param>
    /// <param name="channel"></param>
    public static void Chat(string message, string author = "System", ChatChannel channel = ChatChannel.TCC) 
    {
        if (!message.StartsWith("<font")) message = ChatUtils.Font(message);
        NewChatMessage?.Invoke(channel, message, author);
    }

    public static void MB(string message)
    {
        NewMessageBox?.Invoke(message);
    }
}