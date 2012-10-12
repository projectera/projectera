using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using System.Security;

namespace ProjectERA.Data
{
    [Serializable]
    public class TalentTree
    {
        [ContentSerializer()]
        public Int32 PointId
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializer()]
        public HashSet<Node> Roots
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        internal void AddEdge(Node origin, Node destination)
        {
            // If destination was a root, remove from roots
            if (Roots.Contains(destination))
                Roots.Remove(destination);

            // Add prerequisite to destination
            if (destination != null)
            {
                destination.Parents.Add(origin);
                origin.Children.Add(destination);
            }

            // If origin is a root, add it to roots
            if (origin.Parents.Count == 0)
                this.Roots.Add(origin);
        }

        /// <summary>
        /// Add root to tree
        /// </summary>
        /// <param name="root">root</param>
        internal Boolean AddRoot(Node root)
        {
            // Add it to roots if allowed 
            if (root.Parents.Count == 0)
                return this.Roots.Add(root);

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static TalentTree Generate()
        {
            TalentTree result = new TalentTree();
            result.Roots = new HashSet<Node>();

            return result;
        }

        /// <summary>
        /// Talent
        /// </summary>
        [Serializable]
        public class Node
        {
            /// <summary>
            /// Prerequisities
            /// </summary>
            [ContentSerializer()]
            public List<Node> Parents
            {
                get;
                protected set;
            }

            /// <summary>
            /// Unlockables
            /// </summary>
            [ContentSerializer()]
            public List<Node> Children
            {
                get;
                protected set;
            }

            /// <summary>
            /// Minimum Level
            /// </summary>
            [ContentSerializer()]
            public Int32 Level
            {
                get;
                protected set;
            }

            /// <summary>
            /// Costs
            /// </summary>
            [ContentSerializer()]
            public Int32 Points
            {
                get;
                protected set;
            }

            /// <summary>
            /// Generates a new Talent
            /// </summary>
            /// <param name="level"></param>
            /// <param name="points"></param>
            /// <returns></returns>
            internal static Node Generate(Int32 level, Int32 points)
            {
                Node result = new Node();
                result.Level = level;
                result.Points = points;

                result.Parents = new List<Node>();
                result.Children = new List<Node>();

                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            internal Node Empty()
            {
                return Generate(this.Level, this.Points);
            }
        }

        /// <summary>
        /// Talent that unlocks skill
        /// </summary>
        [Serializable]
        public class SkillNode : Node
        {
            /// <summary>
            /// Skill unlocked
            /// </summary>
            [ContentSerializer()]
            public Int32 Skill
            {
                get;
                private set;
            }

            /// <summary>
            /// Generates a new Skill Talent
            /// </summary>
            /// <param name="level"></param>
            /// <param name="points"></param>
            /// <param name="skill"></param>
            /// <returns></returns>
            internal static Node Generate(Int32 level, Int32 points, Int32 skill)
            {
                SkillNode result = new SkillNode();
                result.Level = level;
                result.Points = points;
                result.Skill = skill;

                result.Parents = new List<Node>();
                result.Children = new List<Node>();

                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            internal new Node Empty()
            {
                return Generate(this.Level, this.Points, this.Skill);
            }
        }

        /// <summary>
        /// Talent that unlocks permanent modifier
        /// </summary>
        [Serializable]
        public class ModifierNode : Node
        {
            /// <summary>
            /// Modifier inflicted
            /// </summary>
            [ContentSerializer()]
            public Int32 Modifier
            {
                get;
                private set;
            }

            /// <summary>
            /// Generates a new Skill Talent
            /// </summary>
            /// <param name="level"></param>
            /// <param name="points"></param>
            /// <param name="skill"></param>
            /// <returns></returns>
            internal static Node Generate(Int32 level, Int32 points, Int32 modifier)
            {
                ModifierNode result = new ModifierNode();
                result.Level = level;
                result.Points = points;
                result.Modifier = modifier;

                result.Parents = new List<Node>();
                result.Children = new List<Node>();

                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            internal new Node Empty()
            {
                return Generate(this.Level, this.Points, this.Modifier);
            }
        }

        /// <summary>
        /// Talent that changes class
        /// </summary>
        [Serializable]
        public class ClassNode : Node
        {
            /// <summary>
            /// Class changed to
            /// </summary>
            [ContentSerializer()]
            public Int32 ClassId
            {
                get;
                private set;
            }

            /// <summary>
            /// Generates a new Skill Talent
            /// </summary>
            /// <param name="level"></param>
            /// <param name="points"></param>
            /// <param name="skill"></param>
            /// <returns></returns>
            internal static Node Generate(Int32 level, Int32 points, Int32 classId)
            {
                ClassNode result = new ClassNode();
                result.Level = level;
                result.Points = points;
                result.ClassId = classId;

                result.Parents = new List<Node>();
                result.Children = new List<Node>();

                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            internal new Node Empty()
            {
                return Generate(this.Level, this.Points, this.ClassId);
            }
        }
    }
}
