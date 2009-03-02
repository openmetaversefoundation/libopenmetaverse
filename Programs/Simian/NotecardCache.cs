using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace Simian
{
    public static class NotecardCache
    {
        class Notecard
        {
            public string[] text;
            public DateTime lastRef;
        }

        static Dictionary<UUID, Notecard> m_Notecards = new Dictionary<UUID, Notecard>();

        public static void Cache(UUID assetID, string text)
        {
            CacheCheck();

            lock (m_Notecards)
            {
                if (m_Notecards.ContainsKey(assetID))
                    return;

                Notecard nc = new Notecard();
                nc.lastRef = DateTime.Now;
                nc.text = ParseText(text.Replace("\r", "").Split('\n'));
                m_Notecards[assetID] = nc;
            }
        }

        public static bool IsCached(UUID assetID)
        {
            lock (m_Notecards)
            {
                return m_Notecards.ContainsKey(assetID);
            }
        }

        public static int GetLines(UUID assetID)
        {
            if (!IsCached(assetID))
                return -1;

            lock (m_Notecards)
            {
                m_Notecards[assetID].lastRef = DateTime.Now;
                return m_Notecards[assetID].text.Length;
            }
        }

        public static string GetLine(UUID assetID, int line)
        {
            if (line < 0)
                return "";

            string data;

            if (!IsCached(assetID))
                return "";

            lock (m_Notecards)
            {
                m_Notecards[assetID].lastRef = DateTime.Now;

                if (line >= m_Notecards[assetID].text.Length)
                    return "\n\n\n";

                data = m_Notecards[assetID].text[line];
                if (data.Length > 255)
                    data = data.Substring(0, 255);

                return data;
            }
        }

        public static void CacheCheck()
        {
            foreach (UUID key in new List<UUID>(m_Notecards.Keys))
            {
                Notecard nc = m_Notecards[key];
                if (nc.lastRef.AddSeconds(30) < DateTime.Now)
                    m_Notecards.Remove(key);
            }
        }

        private static string[] ParseText(string[] input)
        {
            int idx = 0;
            int level = 0;
            List<string> output = new List<string>();
            string[] words;

            while (idx < input.Length)
            {
                if (input[idx] == "{")
                {
                    level++;
                    idx++;
                    continue;
                }

                if (input[idx] == "}")
                {
                    level--;
                    idx++;
                    continue;
                }

                switch (level)
                {
                    case 0:
                        words = input[idx].Split(' '); // Linden text ver
                        // Notecards are created *really* empty. Treat that as "no text" (just like after saving an empty notecard)
                        if (words.Length < 3)
                            return new String[0];

                        int version = int.Parse(words[3]);
                        if (version != 2)
                            return new String[0];
                        break;
                    case 1:
                        words = input[idx].Split(' ');
                        if (words[0] == "LLEmbeddedItems")
                            break;
                        if (words[0] == "Text")
                        {
                            int len = int.Parse(words[2]);
                            idx++;

                            int count = -1;

                            while (count < len)
                            {
                                // int l = input[idx].Length;
                                string ln = input[idx];

                                int need = len - count - 1;
                                if (ln.Length > need)
                                    ln = ln.Substring(0, need);

                                output.Add(ln);
                                count += ln.Length + 1;
                                idx++;
                            }

                            return output.ToArray();
                        }
                        break;
                    case 2:
                        words = input[idx].Split(' '); // count
                        if (words[0] == "count")
                        {
                            int c = int.Parse(words[1]);
                            if (c > 0)
                                return new String[0];
                            break;
                        }
                        break;
                }
                idx++;
            }
            return output.ToArray();
        }
    }
}
