using System;
using System.Collections.Generic;

namespace yxy
{
    /// <summary>
    /// 集合工具类，提供无 GC 分配的 LINQ 替代方法。
    /// 支持 <see cref="List{T}"/> 和数组（<c>T[]</c>）。
    /// </summary>
    /// <remarks>
    /// 注意：方法本身不产生堆分配，但调用方传入的 <see cref="Predicate{T}"/>
    /// 若捕获了外部变量（闭包），仍会在调用侧产生 GC。
    /// 热路径中建议将谓词缓存为静态字段或成员字段。翻译一下就是在频繁调用的地方需要提前创建一个委托来存放Predicate（匹配条件），这样每次lambda传入的委托不会重复创建产生GC
    /// </remarks>
    public static class CollectionHelper
    {
        // ─────────────────────────────────────────────────────────────
        // First
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 返回列表的第一个元素，列表为空时抛出异常。
        /// </summary>
        public static T First<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("CollectionHelper.First: 列表为空或为 null");
            return list[0];
        }

        /// <summary>
        /// 返回数组的第一个元素，数组为空时抛出异常。
        /// </summary>
        public static T First<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                throw new InvalidOperationException("CollectionHelper.First: 数组为空或为 null");
            return array[0];
        }

        /// <summary>
        /// 返回列表中第一个满足条件的元素，不存在时抛出异常。
        /// </summary>
        public static T First<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (match(list[i]))
                    return list[i];
            }
            throw new InvalidOperationException("CollectionHelper.First: 未找到满足条件的元素");
        }

        /// <summary>
        /// 返回数组中第一个满足条件的元素，不存在时抛出异常。
        /// </summary>
        public static T First<T>(this T[] array, Predicate<T> match)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                if (match(array[i]))
                    return array[i];
            }
            throw new InvalidOperationException("CollectionHelper.First: 未找到满足条件的元素");
        }

        // ─────────────────────────────────────────────────────────────
        // FirstOrDefault
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 返回列表的第一个元素，列表为空时返回 <c>default</c>。
        /// </summary>
        public static T FirstOrDefault<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                return default;
            return list[0];
        }

        /// <summary>
        /// 返回数组的第一个元素，数组为空时返回 <c>default</c>。
        /// </summary>
        public static T FirstOrDefault<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                return default;
            return array[0];
        }

        /// <summary>
        /// 返回列表中第一个满足条件的元素，不存在时返回 <c>default</c>。
        /// </summary>
        public static T FirstOrDefault<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null || match == null)
                return default;

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (match(list[i]))
                    return list[i];
            }
            return default;
        }

        /// <summary>
        /// 返回数组中第一个满足条件的元素，不存在时返回 <c>default</c>。
        /// </summary>
        public static T FirstOrDefault<T>(this T[] array, Predicate<T> match)
        {
            if (array == null || match == null)
                return default;

            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                if (match(array[i]))
                    return array[i];
            }
            return default;
        }

        // ─────────────────────────────────────────────────────────────
        // Last
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 返回列表的最后一个元素，列表为空时抛出异常。
        /// </summary>
        public static T Last<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("CollectionHelper.Last: 列表为空或为 null");
            return list[list.Count - 1];
        }

        /// <summary>
        /// 返回数组的最后一个元素，数组为空时抛出异常。
        /// </summary>
        public static T Last<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                throw new InvalidOperationException("CollectionHelper.Last: 数组为空或为 null");
            return array[array.Length - 1];
        }

        /// <summary>
        /// 返回列表中最后一个满足条件的元素，不存在时抛出异常。
        /// </summary>
        public static T Last<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                    return list[i];
            }
            throw new InvalidOperationException("CollectionHelper.Last: 未找到满足条件的元素");
        }

        /// <summary>
        /// 返回数组中最后一个满足条件的元素，不存在时抛出异常。
        /// </summary>
        public static T Last<T>(this T[] array, Predicate<T> match)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = array.Length - 1; i >= 0; i--)
            {
                if (match(array[i]))
                    return array[i];
            }
            throw new InvalidOperationException("CollectionHelper.Last: 未找到满足条件的元素");
        }

        // ─────────────────────────────────────────────────────────────
        // LastOrDefault
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 返回列表的最后一个元素，列表为空时返回 <c>default</c>。
        /// </summary>
        public static T LastOrDefault<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                return default;
            return list[list.Count - 1];
        }

        /// <summary>
        /// 返回数组的最后一个元素，数组为空时返回 <c>default</c>。
        /// </summary>
        public static T LastOrDefault<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                return default;
            return array[array.Length - 1];
        }

        /// <summary>
        /// 返回列表中最后一个满足条件的元素，不存在时返回 <c>default</c>。
        /// </summary>
        public static T LastOrDefault<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null || match == null)
                return default;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                    return list[i];
            }
            return default;
        }

        /// <summary>
        /// 返回数组中最后一个满足条件的元素，不存在时返回 <c>default</c>。
        /// </summary>
        public static T LastOrDefault<T>(this T[] array, Predicate<T> match)
        {
            if (array == null || match == null)
                return default;

            for (int i = array.Length - 1; i >= 0; i--)
            {
                if (match(array[i]))
                    return array[i];
            }
            return default;
        }

        // ─────────────────────────────────────────────────────────────
        // Any
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 判断列表中是否存在满足条件的元素。
        /// </summary>
        public static bool Any<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null || match == null)
                return false;

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (match(list[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断数组中是否存在满足条件的元素。
        /// </summary>
        public static bool Any<T>(this T[] array, Predicate<T> match)
        {
            if (array == null || match == null)
                return false;

            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                if (match(array[i]))
                    return true;
            }
            return false;
        }

        // ─────────────────────────────────────────────────────────────
        // All
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 判断列表中所有元素是否都满足条件。列表为空时返回 <c>true</c>（与 LINQ 行为一致）。
        /// </summary>
        public static bool All<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null || match == null)
                return false;

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (!match(list[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 判断数组中所有元素是否都满足条件。数组为空时返回 <c>true</c>（与 LINQ 行为一致）。
        /// </summary>
        public static bool All<T>(this T[] array, Predicate<T> match)
        {
            if (array == null || match == null)
                return false;

            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                if (!match(array[i]))
                    return false;
            }
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // Count（带谓词）
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 返回列表中满足条件的元素数量，不分配额外内存。
        /// </summary>
        public static int Count<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null || match == null)
                return 0;

            int result = 0;
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (match(list[i]))
                    result++;
            }
            return result;
        }

        /// <summary>
        /// 返回数组中满足条件的元素数量，不分配额外内存。
        /// </summary>
        public static int Count<T>(this T[] array, Predicate<T> match)
        {
            if (array == null || match == null)
                return 0;

            int result = 0;
            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                if (match(array[i]))
                    result++;
            }
            return result;
        }
    }
}
