using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;

namespace Sonic853.Translate.Editor
{
    public class TranslateReader
    {
        public TranslateReader(string path)
        {
            ReadFile(path);
        }
        private string[] lines = new string[0];
        private readonly List<string> msgid = new();
        private readonly List<string> msgstr = new();
        public string language = "en_US";
        public string lastTranslator = "anonymous";
        public string languageTeam = "anonymous";
        public string[] ReadFile(string path, bool parse = true)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"File {path} not found");
                return null;
            }
            var text = File.ReadAllText(path);
            lines = text.Split('\n');
            if (text.Contains("\r\n"))
                lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            if (parse)
                ParseFile(lines);
            return lines;
        }
        public void ParseFile(string path)
        {
            ReadFile(path, true);
        }
        public void ParseFile(string[] _lines)
        {
            msgid.Clear();
            msgstr.Clear();
            var msgidIndex = -1;
            var msgstrIndex = -1;
            var msgidStr = "msgid \"";
            var msgidLength = msgidStr.Length;
            var msgstrStr = "msgstr \"";
            var msgstrLength = msgstrStr.Length;
            var languageStr = "\"Language: ";
            var languageLength = languageStr.Length;
            var lastTranslatorStr = "\"Last-Translator: ";
            var lastTranslatorLength = lastTranslatorStr.Length;
            var languageTeamStr = "\"Language-Team: ";
            var languageTeamLength = languageTeamStr.Length;
            var doubleQuotationStr = "\"";
            var doubleQuotationLength = doubleQuotationStr.Length;
            foreach (var line in _lines)
            {
                var _line = line.Trim();
                if (_line.StartsWith(msgidStr))
                {
                    msgid.Add(ReturnText(_line[msgidLength.._line.LastIndexOf('"')]));
                    msgidIndex = msgid.Count - 1;
                    msgstrIndex = -1;
                    continue;
                }
                if (_line.StartsWith(msgstrStr))
                {
                    msgstr.Add(ReturnText(_line[msgstrLength.._line.LastIndexOf('"')]));
                    msgstrIndex = msgstr.Count - 1;
                    continue;
                }
                if (_line.StartsWith(languageStr) && msgstrIndex == 0)
                {
                    language = _line[languageLength.._line.LastIndexOf('"')];
                    // 找到并去除\n
                    if (language.Contains("\\n"))
                        language = language.Replace("\\n", "");
                    continue;
                }
                if (_line.StartsWith(lastTranslatorStr) && msgstrIndex == 0)
                {
                    lastTranslator = _line[lastTranslatorLength.._line.LastIndexOf('"')];
                    // 找到并去除\n
                    if (lastTranslator.Contains("\\n"))
                        lastTranslator = lastTranslator.Replace("\\n", "");
                    // 将<和>替换为＜和＞
                    lastTranslator = lastTranslator.Replace("<", "＜").Replace(">", "＞");
                    continue;
                }
                if (_line.StartsWith(languageTeamStr) && msgstrIndex == 0)
                {
                    languageTeam = _line[languageTeamLength.._line.LastIndexOf('"')];
                    // 找到并去除\n
                    if (languageTeam.Contains("\\n"))
                        languageTeam = languageTeam.Replace("\\n", "");
                    // 将<和>替换为＜和＞
                    languageTeam = languageTeam.Replace("<", "＜").Replace(">", "＞");
                    continue;
                }
                if (_line.StartsWith(doubleQuotationStr))
                {
                    if (msgidIndex != -1 && msgidIndex != 0)
                    {
                        msgid[msgidIndex] += ReturnText(_line[doubleQuotationLength.._line.LastIndexOf('"')]);
                        continue;
                    }
                    if (msgstrIndex != -1 && msgstrIndex != 0)
                    {
                        msgstr[msgstrIndex] += ReturnText(_line[doubleQuotationLength.._line.LastIndexOf('"')]);
                        continue;
                    }
                }
            }
            if (msgid.Count != msgstr.Count)
            {
                Debug.LogError("msgid.Count != msgstr.Count");
                return;
            }
        }
        string ReturnText(string text)
        {
            if (text.EndsWith("\\\\n"))
            {
                text = text[..^3];
                text += "\\n";
            }
            else if (text.EndsWith("\\n"))
            {
                text = text[..^2];
                text += "\n";
            }
            return text;
        }
        public string GetText(string text)
        {
            var index = msgid.IndexOf(text);
            if (index == -1)
                return text;
            return string.IsNullOrWhiteSpace(msgstr[index]) ? text : msgstr[index];
        }
        public string _(string text)
        {
            return GetText(text);
        }
    }
}
