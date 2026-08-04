using System;
using System.Collections.Generic;
using System.IO;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Tests.Core
{
    public class CorePersistenceTests
    {
        [Fact]
        public void MemoryRoundTripPreservesScalarsCollectionsAndMatrix()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string document = CreateDocument();
            Assert.Contains("# managed persistence test", document);
            Assert.Contains("managed", document);

            using var reader = new FileStorage();
            Assert.False(reader.IsOpened);
            Assert.True(reader.Open(document, FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml));
            Assert.True(reader.IsOpened);
            Assert.Equal(FileStorageModes.FormatYaml, reader.GetFormat());

            using FileNode first = reader.GetFirstTopLevelNode();
            Assert.Equal("count", first.Name);
            Assert.Equal(7.0, first.Real);
            using FileNode count = reader["count"];
            using FileNode enabled = reader["enabled"];
            using FileNode large = reader["large"];
            using FileNode score = reader["score"];
            using FileNode text = reader["text"];
            Assert.True(count.IsInteger);
            Assert.Equal(7.0, count.Real);
            Assert.Equal(1.0, enabled.Real);
            Assert.Equal(1234567890123.0, large.Real);
            Assert.Equal(2.5, score.Real);
            Assert.Equal("h\u00e9llo-\u4e2d\u6587-\U0001F642", text.String);

            using FileNode root = reader.Root();
            Assert.True(root.IsMap);
            Assert.Equal(new[] { "count", "enabled", "large", "score", "text", "matrix", "words", "emptyWords", "metadata", "values" }, root.Keys);

            using FileNode words = reader["words"];
            Assert.True(words.IsSequence);
            Assert.Equal(3, words.Size);
            using FileNode firstWord = words[0];
            using FileNode emptyWord = words[1];
            using FileNode lastWord = words[2];
            Assert.Equal("alpha", firstWord.String);
            Assert.Equal(string.Empty, emptyWord.String);
            Assert.Equal("\u4e59", lastWord.String);
            Assert.Equal(3, words.CountChildren());

            using FileNode emptyWords = reader["emptyWords"];
            Assert.Equal(2, emptyWords.Size);
            using FileNode emptyWord0 = emptyWords[0];
            using FileNode emptyWord1 = emptyWords[1];
            Assert.Equal(string.Empty, emptyWord0.String);
            Assert.Equal(string.Empty, emptyWord1.String);

            using FileNode metadata = reader["metadata"];
            Assert.True(metadata.IsMap);
            Assert.Equal(new[] { "owner" }, metadata.Keys);
            using FileNode owner = metadata["owner"];
            Assert.Equal("managed", owner.String);

            using FileNode values = reader["values"];
            Assert.Equal(2, values.Size);
            using FileNode secondValue = values[1];
            Assert.Equal(13.0, secondValue.Real);

            using FileNode matrixNode = reader["matrix"];
            using Mat matrix = matrixNode.ToMat();
            Assert.Equal(2, matrix.Rows);
            Assert.Equal(2, matrix.Cols);
            Assert.Equal(MatType.CV_32SC1, matrix.Type);
            Assert.Equal(new[] { 2, 4, 6, 8 }, matrix.ToArray<int>());
        }

        [Fact]
        public void Utf8FilePathRoundTripUsesExactText()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), "jyppx-persistence-\u4e2d\u6587-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "\u914d\u7f6e.yml");
            Directory.CreateDirectory(directory);
            try
            {
                using (var writer = new FileStorage(path, FileStorageModes.Write | FileStorageModes.FormatYaml))
                {
                    Assert.True(writer.IsOpened);
                    writer.Write("message", "\u8def\u5f84-\U0001F642");
                }

                using var reader = new FileStorage(path, FileStorageModes.Read);
                Assert.True(reader.IsOpened);
                Assert.Equal(FileStorageModes.FormatYaml, reader.GetFormat());
                using FileNode message = reader["message"];
                Assert.Equal("\u8def\u5f84-\U0001F642", message.String);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void NodeKeepsStorageAliveAfterParentDispose()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string document = CreateDocument();
            var reader = new FileStorage(document, FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml);
            FileNode node = reader["score"];
            reader.Dispose();
            Assert.True(reader.IsDisposed);
            Assert.Equal(2.5, node.Real);
            node.Dispose();
            node.Dispose();
            Assert.True(node.IsDisposed);
        }

        [Fact]
        public void ReleaseAndReopenInvalidatePreviouslyObtainedNodes()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string document = CreateDocument();
            using var reader = new FileStorage(document, FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml);
            using FileNode stale = reader["score"];
            reader.Release();
            Assert.False(reader.IsOpened);
            OpenCvException releaseException = Assert.Throws<OpenCvException>(() => stale.Real);
            Assert.Contains("invalidated", releaseException.Message, StringComparison.OrdinalIgnoreCase);

            Assert.True(reader.Open(document, FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml));
            Assert.Throws<OpenCvException>(() => stale.Real);
            using FileNode current = reader["score"];
            Assert.Equal(2.5, current.Real);
        }

        [Fact]
        public void EmptyWrongTypeInvalidArgumentsAndDisposedHandlesFailCleanly()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var empty = new FileNode())
            {
                Assert.True(empty.Empty);
                Assert.True(empty.IsNone);
                Assert.False(empty.IsNamed);
                Assert.Equal(string.Empty, empty.Name);
            }

            string document = CreateDocument();
            using var reader = new FileStorage(document, FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml);
            using FileNode text = reader["text"];
            using FileNode words = reader["words"];
            using FileNode missing = reader["missing"];
            Assert.True(missing.Empty);
            Assert.Throws<InvalidOperationException>(() => text.Keys);
            Assert.Throws<InvalidOperationException>(() => text[0]);
            Assert.Throws<InvalidOperationException>(() => text.Real);
            Assert.Throws<OpenCvException>(() => text.ToMat());
            Assert.Throws<ArgumentOutOfRangeException>(() => words[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => words[words.Size]);

            using var closed = new FileStorage();
            Assert.Throws<ArgumentException>(() => closed.Open("bad\0path", FileStorageModes.Read));
            Assert.Throws<ArgumentOutOfRangeException>(() => closed.Open("memory.yml", (FileStorageModes)3));
            Assert.Throws<ArgumentOutOfRangeException>(() => closed.Root(-1));
            Assert.Throws<InvalidOperationException>(() => closed.Write("value", 1));
            Assert.Throws<ArgumentNullException>(() => closed.Open(null!, FileStorageModes.Read));

            using var writer = new FileStorage("memory.yml", FileStorageModes.Write | FileStorageModes.Memory | FileStorageModes.FormatYaml);
            Assert.Throws<ArgumentException>(() => writer.Write("bad\0name", 1));
            Assert.Throws<ArgumentException>(() => writer.Write("value", "bad\0value"));
            Assert.Throws<ArgumentException>(() => writer.Write("values", new string[] { "ok", null! }));
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.StartWriteStruct("bad", FileNodeTypes.Integer));

            var disposedStorage = new FileStorage();
            disposedStorage.Dispose();
            disposedStorage.Dispose();
            Assert.True(disposedStorage.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => disposedStorage.IsOpened);

            var disposedNode = new FileNode();
            disposedNode.Dispose();
            disposedNode.Dispose();
            Assert.True(disposedNode.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => disposedNode.Empty);
        }

        [Fact]
        public void RepeatedRoundTripsAndOwnedResultsDisposeIndependently()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            for (int i = 0; i < 16; i++)
            {
                string document = CreateDocument();
                using var reader = new FileStorage(document, FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml);
                using FileNode root = reader.Root();
                string[] keys = root.Keys;
                Assert.Equal(10, keys.Length);
                using FileNode matrixNode = reader["matrix"];
                using Mat matrix = matrixNode.ToMat();
                Assert.Equal(new[] { 2, 4, 6, 8 }, matrix.ToArray<int>());
            }
        }

        private static string CreateDocument()
        {
            using var matrix = new Mat(2, 2, MatType.CV_32SC1);
            matrix.CopyFrom(new[] { 2, 4, 6, 8 });
            using var writer = new FileStorage("memory.yml", FileStorageModes.Write | FileStorageModes.Memory | FileStorageModes.FormatYaml);
            writer.WriteComment("managed persistence test");
            writer.Write("count", 7);
            writer.Write("enabled", true);
            writer.Write("large", 1234567890123L);
            writer.Write("score", 2.5);
            writer.Write("text", "h\u00e9llo-\u4e2d\u6587-\U0001F642");
            writer.Write("matrix", matrix);
            writer.Write("words", new[] { "alpha", string.Empty, "\u4e59" });
            writer.Write("emptyWords", new[] { string.Empty, string.Empty });
            writer.StartWriteStruct("metadata", FileNodeTypes.Map);
            writer.Write("owner", "managed");
            writer.EndWriteStruct();
            writer.StartWriteStruct("values", FileNodeTypes.Sequence);
            writer.Write(string.Empty, 11);
            writer.Write(string.Empty, 13);
            writer.EndWriteStruct();
            return writer.ReleaseAndGetString();
        }
    }

    internal static class FileNodeTestExtensions
    {
        internal static int CountChildren(this FileNode node)
        {
            int count = 0;
            foreach (FileNode child in node)
            {
                using (child)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
