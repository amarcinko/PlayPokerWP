using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    static class Check
    {
        internal static int[] patternArray(string[] selected)
        {
            int[] checkArray = new int[5];
            string tempString;
            int num = 0;
            int[] patternArray = new int[5];

            for (int i = 0; i < 5; i++)
            {
                tempString = selected[i].Remove(selected[i].Length - 1);
                if (tempString == "A") { checkArray[i] = 14; }
                else if (tempString == "K") { checkArray[i] = 13; }
                else if (tempString == "Q") { checkArray[i] = 12; }
                else if (tempString == "J") { checkArray[i] = 11; }
                else
                {
                    int.TryParse(tempString, out checkArray[i]);
                }
            }

            for (int i = 0; i < checkArray.Length; i++)
            {
                num = 0;
                for (int s = 0; s < checkArray.Length; s++)
                {
                    if (checkArray[s] == checkArray[i])
                    { num++; }
                }
                patternArray[i] = num;
            }

            Array.Sort(patternArray);

            return patternArray;
        }

        internal static bool OnePair(string[] selected, int[] patternArray)//za multiplayer
        {
            int[] jp = { 1, 1, 1, 2, 2 };

            if (StructuralComparisons.StructuralEqualityComparer.Equals(patternArray, jp))
            {
                return true;
            }
            else return false;
        }

        internal static bool JacksOrBetter(string[] selected, int[] patternArray)
        {
            string[] jobArray = new string[5];
            int[] jp = { 1, 1, 1, 2, 2 };

            for (int i = 0; i < 5; i++)
            {
                jobArray[i] = selected[i].Remove(selected[i].Length - 1);
            }

            var groups = jobArray.GroupBy(m => m);
            foreach (var group in groups)
            {
                if (group.Count() == 2 && (group.Key == "J" || group.Key == "Q" || group.Key == "K" || group.Key == "A")
                    && StructuralComparisons.StructuralEqualityComparer.Equals(patternArray, jp))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool TwoPairs(string[] selected, int[] patternArray)
        {
            int[] dp = { 1, 2, 2, 2, 2 };

            if (StructuralComparisons.StructuralEqualityComparer.Equals(patternArray, dp))
            {
                return true;
            }
            else return false;
        }

        internal static bool Blaze(string[] selected, int[] patternArray)
        {
            int[] dp = { 1, 2, 2, 2, 2 };

            if (StructuralComparisons.StructuralEqualityComparer.Equals(patternArray, dp))
            {
                bool blaze = true;
                string[] tmpBlazeArray = new string[5];
                string[] sampleNonBlaze = {"1", "2", "3", "4", "5", "6", "7", "8",
                                        "9", "10", "A"};
                for (int i = 0; i < 5; i++)
                { tmpBlazeArray[i] = selected[i].Remove(selected[i].Length - 1); }

                foreach (string x in sampleNonBlaze)
                {
                    if (tmpBlazeArray.Contains(x))
                    {
                        blaze = false;
                        break;
                    }
                }
                return blaze;
            }
            else return false;
            
        }

        internal static bool ThreeOfAKind(string[] selected, int[] patternArray)
        {
            int[] tr = { 1, 1, 3, 3, 3 };

            if (StructuralComparisons.StructuralEqualityComparer.Equals(patternArray, tr))
            {
                return true;
            }
            else return false;
        }

        internal static bool Straight(string[] selected)
        {
            string[] tmpStraightArray = new string[5];
            int[] straightArray = new int[5];

            for (int i = 0; i < 5; i++)
            { tmpStraightArray[i] = selected[i].Remove(selected[i].Length - 1); }

            for (int i = 0; i < 5; i++)
            {
                if (tmpStraightArray[i] == "A") straightArray[i] = 14;
                else if (tmpStraightArray[i] == "K") straightArray[i] = 13;
                else if (tmpStraightArray[i] == "Q") straightArray[i] = 12;
                else if (tmpStraightArray[i] == "J") straightArray[i] = 11;
                else
                {
                    Int32.TryParse(tmpStraightArray[i], out straightArray[i]);
                }
            }

            Array.Sort(straightArray);
            int[] aStr = { 2, 3, 4, 5, 14 };
            if (StructuralComparisons.StructuralEqualityComparer.Equals(straightArray, aStr))
            {
                return true;
            }
            bool retStraight = false;
            for (int c = 4; c > 0; c--)
            {
                if ((straightArray[c] - straightArray[c - 1]) == 1) { retStraight = true; }
                else { retStraight = false; break; }
            }
            return retStraight;
        }

        internal static bool CornerStraight(string[] selected)
        {
            string[] tmpStraightArray = new string[5];
            int[] straightArray = new int[5];

            for (int i = 0; i < 5; i++)
            { tmpStraightArray[i] = selected[i].Remove(selected[i].Length - 1); }

            for (int i = 0; i < 5; i++)
            {
                if (tmpStraightArray[i] == "A") straightArray[i] = 14;
                else if (tmpStraightArray[i] == "K") straightArray[i] = 13;
                else if (tmpStraightArray[i] == "Q") straightArray[i] = 12;
                else if (tmpStraightArray[i] == "J") straightArray[i] = 11;
                else
                {
                    Int32.TryParse(tmpStraightArray[i], out straightArray[i]);
                }
            }

            Array.Sort(straightArray);
            List<Array> cStr = new List<Array>();
            int[] cStr1 = { 2, 3, 4, 13, 14 };
            int[] cStr2 = { 2, 3, 12, 13, 14 };
            int[] cStr3 = { 2, 11, 12, 13, 14 };
            cStr.Add(cStr1);
            cStr.Add(cStr2);
            cStr.Add(cStr3);
            foreach (var x in cStr)
            {
                if (StructuralComparisons.StructuralEqualityComparer.Equals(straightArray, x))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool KangarooStraight(string[] selected)
        {
            string[] tmpStraightArray = new string[5];
            int[] straightArray = new int[5];

            for (int i = 0; i < 5; i++)
            { tmpStraightArray[i] = selected[i].Remove(selected[i].Length - 1); }

            for (int i = 0; i < 5; i++)
            {
                if (tmpStraightArray[i] == "A") straightArray[i] = 14;
                else if (tmpStraightArray[i] == "K") straightArray[i] = 13;
                else if (tmpStraightArray[i] == "Q") straightArray[i] = 12;
                else if (tmpStraightArray[i] == "J") straightArray[i] = 11;
                else
                {
                    Int32.TryParse(tmpStraightArray[i], out straightArray[i]);
                }
            }

            Array.Sort(straightArray);
            int[] aStr = { 3, 5, 7, 9, 14 };
            if (StructuralComparisons.StructuralEqualityComparer.Equals(straightArray, aStr))
            {
                return true;
            }
            bool retStraight = false;
            for (int c = 4; c > 0; c--)
            {
                if ((straightArray[c] - straightArray[c - 1]) == 2) { retStraight = true; }
                else { retStraight = false; break; }
            }
            return retStraight;
        }

        internal static bool Flush(string[] selected)
        {
            char[] flushArray = new char[5];
            for (int i = 0; i < 5; i++)
            {
                flushArray[i] = selected[i][selected[i].Length - 1];
            }

            if (flushArray[0] == flushArray[1] && flushArray[0] == flushArray[2]
                && flushArray[0] == flushArray[3] && flushArray[0] == flushArray[4]) return true;
            else return false;
        }

        internal static bool FullHouse(string[] selected, int[] patternArray)
        {
            int[] ful = { 2, 2, 3, 3, 3 };

            if (StructuralComparisons.StructuralEqualityComparer.Equals(patternArray, ful))
            {
                return true;
            }
            else return false;
        }

        internal static bool Poker(string[] selected, int[] patternArray)
        {
            int[] po = { 1, 4, 4, 4, 4 };

            if (StructuralComparisons.StructuralEqualityComparer.Equals(patternArray, po))
            {
                return true;
            }
            else return false;
        }

        internal static int Max(string[] selected)
        {
            string[] tmpStraightArray = new string[5];
            int[] straightArray = new int[5];

            for (int i = 0; i < 5; i++)
            { tmpStraightArray[i] = selected[i].Remove(selected[i].Length - 1); }

            for (int i = 0; i < 5; i++)
            {
                if (tmpStraightArray[i] == "A") straightArray[i] = 14;
                else if (tmpStraightArray[i] == "K") straightArray[i] = 13;
                else if (tmpStraightArray[i] == "Q") straightArray[i] = 12;
                else if (tmpStraightArray[i] == "J") straightArray[i] = 11;
                else
                {
                    Int32.TryParse(tmpStraightArray[i], out straightArray[i]);
                }
            }

            return straightArray.Max();
        }

        internal static string Both_have_same(string[] master, string[] slave)
        {
            string[] tmpMaster = new string[5];
            string[] tmpSlave = new string[5];
            int[] tmpMasterInt = new int[5];
            int[] tmpSlaveInt = new int[5];

            for (int i = 0; i < 5; i++)
            { 
                tmpMaster[i] = master[i].Remove(master[i].Length - 1);
                tmpSlave[i] = slave[i].Remove(slave[i].Length - 1);
            }

            for (int i = 0; i < 5; i++)
            {
                if (tmpMaster[i] == "A") tmpMasterInt[i] = 14;
                else if (tmpMaster[i] == "K") tmpMasterInt[i] = 13;
                else if (tmpMaster[i] == "Q") tmpMasterInt[i] = 12;
                else if (tmpMaster[i] == "J") tmpMasterInt[i] = 11;
                else
                {
                    Int32.TryParse(tmpMaster[i], out tmpMasterInt[i]);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                if (tmpSlave[i] == "A") tmpSlaveInt[i] = 14;
                else if (tmpSlave[i] == "K") tmpSlaveInt[i] = 13;
                else if (tmpSlave[i] == "Q") tmpSlaveInt[i] = 12;
                else if (tmpSlave[i] == "J") tmpSlaveInt[i] = 11;
                else
                {
                    Int32.TryParse(tmpSlave[i], out tmpSlaveInt[i]);
                }
            }

            Dictionary<int, int> dic_m = new Dictionary<int, int>();
            foreach (int num in tmpMasterInt)
            {
                if (dic_m.ContainsKey(num))
                {
                    dic_m[num] += 1; 
                }
                else
                {
                    dic_m.Add(num,1);
                }
            }

            Dictionary<int, int> dic_s = new Dictionary<int, int>();
            foreach (int num in tmpSlaveInt)
            {
                if (dic_s.ContainsKey(num))
                {
                    dic_s[num] += 1;
                }
                else
                {
                    dic_s.Add(num, 1);
                }
            }

            if (dic_m.Count == 4)
            {
                int m = dic_m.FirstOrDefault(x => x.Value == 2).Key;
                int s = dic_s.FirstOrDefault(x => x.Value == 2).Key;
                if (m > s) return "m";
                else if (m < s) return "s";
                else return "n"; 
            }
            else if (dic_m.Count == 3)
            {
                int m = dic_m.FirstOrDefault(x => x.Value == 3).Key;
                int s = dic_s.FirstOrDefault(x => x.Value == 3).Key;
                if (m > s) return "m";
                else if (m < s) return "s";
                else return "n";
            }
            else if (dic_m.Count == 2)
            {
                int m = dic_m.FirstOrDefault(x => x.Value == 4).Key;
                int s = dic_s.FirstOrDefault(x => x.Value == 4).Key;
                if (m > s) return "m";
                else if (m < s) return "s";
                else return "n";
            }
            else return "n";
        }

    }
}
