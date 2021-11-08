using System;

namespace EditorHelper.Utils {
    public static class Similarity {
        public static int GetDistance(this string s1, string s2) {
            var dist = new int[s1.Length + 1,  s2.Length + 1];
            for (int i = 0; i <= s1.Length; i++) {
                dist[i, 0] = i;
            }
            for (int j = 0; j <= s2.Length; j++) {
                dist[0, j] = j;
            }

            for (int j = 1; j <= s2.Length; j++) {
                for (int i = 1; i <= s1.Length; i++) {
                    if (s1[i-1] == s2[j-1]) dist[i, j] = dist[i-1, j-1];
                    else dist[i, j] = Math.Min(dist[i-1, j], Math.Min(dist[i, j-1], dist[i-1, j-1])) + 1;
                }
            }

            return dist[s1.Length, s2.Length];
        }

        public static decimal GetSimilarity(this string s1, string s2) {
            return 1 - s1.GetDistance(s2) / (decimal) Math.Max(s1.Length, s2.Length);
        }
        public static double GetInclusion(this string target, string search) {
            if (search.Length == 0) return target.Length == 0 ? Double.PositiveInfinity : 1;
            var distance = GetDistance(search, target);
            var diff = (double) (target.Length - search.Length);
            if (diff == 0) diff = double.Epsilon;
            var sim = diff / distance;
            var div = 1 - (double) search.Length / target.Length;
            return (sim - div) / (1 - div);
        }
    }
}