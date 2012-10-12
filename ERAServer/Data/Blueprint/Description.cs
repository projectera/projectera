using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ERAServer.Data.Blueprint
{
    internal class Description
    {
        public static readonly Description Empty = Generate("No description.");

        /// <summary>
        /// 
        /// </summary>
        public String Contents
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String ParsedContents
        {
            get
            {
                String result = this.Contents;
                String blockIds = String.Join("", Item.BlockId, Equipment.BlockId, Skill.BlockId);
                foreach (Match a in Regex.Matches(this.Contents, Regex.Escape("[") + "([" + blockIds + "]+)([bnc]):(.*?)" + Regex.Escape("]") , RegexOptions.Singleline | RegexOptions.Compiled))
                {
                    String matched = a.Groups[0].Value;
                    String parsed = matched;
                    switch (a.Groups[1].Value)
                    {
                        case Item.BlockId:
                            parsed = Item.Parse(matched);
                            break;

                        case Equipment.BlockId:
                            parsed = Equipment.Parse(matched);
                            break;

                        case Skill.BlockId:
                            parsed = Skill.Parse(matched);
                            break;
                    }

                    result = result.Replace(matched, parsed);
                }
                
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Description()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static Description Generate(String contents)
        {
            Description result = new Description();
            result.Contents = contents;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public class Item : ParsableBlock
        {
            public const String BlockId = "I";

            /// <summary>
            /// 
            /// </summary>
            /// <param name="contents"></param>
            /// <returns></returns>
            public static String Parse(String contents)
            {
                String replacementOnFailure = "some item";
                String displayAs = "]";
                Blueprint.Item item = null;

                if (contents.Split('|').Length > 1)
                {
                    String[] parts = contents.Split('|');
                    displayAs = parts[1];
                    contents = parts[0];
                    replacementOnFailure = displayAs.Substring(1, displayAs.Length - 2);
                }

                if (contents.StartsWith(BlockId + SuffixBlueprintId + ":"))
                {
                    Int32 id; Int32.TryParse(contents.Replace(BlockId + SuffixBlueprintId + ":", ""), out id);
                    item = Blueprint.Item.GetBlocking(id);
                }

                if (contents.StartsWith(BlockId + SuffixName + ":"))
                {
                    String name = contents.Replace(BlockId + SuffixName + ":", "");
                    item = Blueprint.Item.GetBlocking(name);

                    if (replacementOnFailure == "some item")
                        replacementOnFailure = name;
                }

                if (item != null)
                    return String.Format("[{3}{4}:{0}|{1}{2}", item.Id, item.Name, displayAs, BlockId, SuffixContentId);

                return replacementOnFailure;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Equipment : ParsableBlock
        {
            public const String BlockId = "E";

            /// <summary>
            /// 
            /// </summary>
            /// <param name="contents"></param>
            /// <returns></returns>
            public static String Parse(String contents)
            {
                String replacementOnFailure = "some equipment";
                String displayAs = "]";
                Blueprint.Equipment item = null;

                if (contents.Split('|').Length > 1)
                {
                    String[] parts = contents.Split('|');
                    displayAs = parts[1];
                    contents = parts[0];
                    replacementOnFailure = displayAs.Substring(1, displayAs.Length - 2);
                }

                try
                {
                    if (contents.StartsWith(BlockId + SuffixBlueprintId + ":"))
                    {
                        Int32 id; Int32.TryParse(contents.Replace(BlockId + SuffixBlueprintId + ":", ""), out id);
                        item = Blueprint.Equipment.GetBlocking(id);
                    }

                    if (contents.StartsWith(BlockId + SuffixName + ":"))
                    {
                        String name = contents.Replace(BlockId + SuffixName + ":", "");
                        item = Blueprint.Equipment.GetBlocking(name);

                        if (replacementOnFailure == "some equipment")
                            replacementOnFailure = name;
                    }

                    if (item != null)
                        return String.Format("[{3}{4}:{0}|{1}{2}", item.Id, item.Name, displayAs, BlockId, SuffixContentId);

                    return replacementOnFailure;
                }
                catch (Exception)
                {
                    return replacementOnFailure;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Skill : ParsableBlock
        {
            public const String BlockId = "S";

            /// <summary>
            /// 
            /// </summary>
            /// <param name="contents"></param>
            /// <returns></returns>
            public static String Parse(String contents)
            {
                String replacementOnFailure = "some skill";
                String displayAs = "]";
                Blueprint.Skill item = null;

                if (contents.Split('|').Length > 1)
                {
                    String[] parts = contents.Split('|');
                    displayAs = parts[1];
                    contents = parts[0];
                    replacementOnFailure = displayAs.Substring(1, displayAs.Length - 2);
                }

                try
                {
                    if (contents.StartsWith(BlockId + SuffixBlueprintId + ":"))
                    {
                        Int32 id; Int32.TryParse(contents.Replace(BlockId + SuffixBlueprintId + ":", ""), out id);
                        item = Blueprint.Skill.GetBlocking(id);
                    }

                    if (contents.StartsWith(BlockId + SuffixName + ":"))
                    {
                        String name = contents.Replace(BlockId + SuffixName + ":", "");
                        item = Blueprint.Skill.GetBlocking(name);

                        if (replacementOnFailure == "some skill")
                            replacementOnFailure = name;
                    }

                    
                    if (item != null)
                        return String.Format("[{3}{4}:{0}|{1}{2}", item.Id, item.Name, displayAs, BlockId, SuffixContentId);

                    return replacementOnFailure;
                }
                catch (Exception)
                {
                    return replacementOnFailure;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal abstract class ParsableBlock
        {
            protected const String SuffixBlueprintId = "b";
            protected const String SuffixContentId = "c";
            protected const String SuffixName = "n";

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public static String Generate(String pbId, Int32 id)
            {
                return String.Format("[{1}{2}:{0}]", id, pbId, SuffixContentId);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <param name="display"></param>
            /// <returns></returns>
            internal static String Generate(String pbId, Int32 id, String display)
            {
                return String.Format("[{2}{3}:{0}|{1}]", id, display, pbId, SuffixContentId);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public static String Generate(String pbId, ObjectId id)
            {
                return String.Format("[{1}{2}:{0}]", id, pbId, SuffixBlueprintId);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <param name="display"></param>
            /// <returns></returns>
            internal static String Generate(String pbId, ObjectId id, String display)
            {
                return String.Format("[{2}{3}:{0}|{1}]", id, display, pbId, SuffixBlueprintId);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public static String Generate(String pbId, String name)
            {
                return String.Format("[{1}{2}:{0}]", name, pbId, SuffixName);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <param name="display"></param>
            /// <returns></returns>
            internal static String Generate(String pbId, String name, String display)
            {
                return String.Format("[{2}{3}:{0}|{1}]", name, display, pbId, SuffixName);
            }
        }
    }
}
