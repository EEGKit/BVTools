﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrainVision.Lab.FileFormats.PublicDomain.XSV;

namespace BrainVision.Lab.FileFormats.PublicDomain.BidsFormat.Internal.IO
{
    internal static class TsvTableWriter<T> where T : notnull, IConvertibleToStringTable
    {
        public static void Save(string filePath, IReadOnlyList<string> header, List<T> items)
        {
            List<string> headerRow = header.ToList();
            List<string?>[] tableRows = ConvertItemsToStrings(items);

            List<int> indicesOfInvalidColumns = GetIndicesOfColumnsContainingNoData(tableRows);
            indicesOfInvalidColumns.Sort();

            RemoveInvalidColumnsFromRow(headerRow, indicesOfInvalidColumns);
            RemoveInvalidColumnsFromTable(tableRows, indicesOfInvalidColumns);
            ReplaceNullsWithEmptyStrings(tableRows);

            Save(filePath, headerRow, tableRows!);
        }

        private static List<string?>[] ConvertItemsToStrings(List<T> items)
        {
            List<string?>[] rows = new List<string?>[items.Count];

            for (int i = 0; i < items.Count; ++i)
            {
                T item = items[i];
                rows[i] = item.ToList();
            }

            return rows;
        }

        /// <summary>
        /// Columns with data are columns that contain at least one object (not null)
        /// </summary>
        private static List<int> GetIndicesOfColumnsContainingData(List<string?>[] rows)
        {
            List<int> indicesOfValidColumns = new List<int>();

            foreach (List<string?> row in rows)
            {
                for (int i = 0; i < row.Count; ++i)
                {
                    if (row[i] != null)
                    {
                        if (!indicesOfValidColumns.Contains(i))
                            indicesOfValidColumns.Add(i);
                    }
                }
            }

            return indicesOfValidColumns;
        }

        /// <summary>
        /// Columns without data are columns that contain only nulls
        /// </summary>
        private static List<int> GetIndicesOfColumnsContainingNoData(List<string?>[] rows)
        {
            List<int> indicesOfValidColumns = GetIndicesOfColumnsContainingData(rows);
            List<int> indicesOfInvalidColumns = new List<int>();

            int channelCount = rows.Length == 0 ? 0 : rows[0].Count;
            for (int i = 0; i < channelCount; ++i)
            {
                if (!indicesOfValidColumns.Contains(i))
                    indicesOfInvalidColumns.Add(i);
            }

            return indicesOfInvalidColumns;
        }

        private static void RemoveInvalidColumnsFromTable(IList[] tableRows, List<int> indicesOfInvalidColumns)
        {
            foreach (IList row in tableRows)
                RemoveInvalidColumnsFromRow(row, indicesOfInvalidColumns);
        }

        private static void RemoveInvalidColumnsFromRow(IList list, List<int> indicesOfInvalidColumns)
        {
            // assuming that indicesOfInvalidColumns are sorted
            for (int i = indicesOfInvalidColumns.Count - 1; i >= 0; --i)
            {
                int index = indicesOfInvalidColumns[i];
                list.RemoveAt(index);
            }
        }

        private static void ReplaceNullsWithEmptyStrings(List<string?>[] tableRows)
        {
            foreach (List<string?> row in tableRows)
                ReplaceNullsWithEmptyStrings(row);
        }

        private static void ReplaceNullsWithEmptyStrings(List<string?> row)
        {
            for (int i = 0; i < row.Count; ++i)
            {
                row[i] ??= string.Empty;
            }
        }

        private static void Save(string filePath, List<string> headerRow, List<string>[] tableRows)
        {
            using IXsvWriter<string> tsv = XsvFactory.CreateWriter<string>(filePath, '\t');
            tsv.WriteHeader(headerRow);
            tsv.Write(tableRows);
        }
    }
}
