using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Owns an OpenCV Delaunay subdivision and its quad-edge topology.
    /// 拥有一个 OpenCV Delaunay 细分及其 quad-edge 拓扑。
    /// </summary>
    public sealed unsafe class Subdiv2D : IDisposable
    {
        private NativeSubdiv2DHandle handle;
        private bool disposed;

        /// <summary>Creates an uninitialized subdivision. 创建尚未初始化的细分。</summary>
        public Subdiv2D()
        {
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DCreate(out IntPtr nativeHandle));
            handle = NativeSubdiv2DHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Creates a subdivision for an integer reference rectangle. 为整数参考矩形创建细分。</summary>
        public Subdiv2D(Rect rect)
        {
            ValidateRect(rect);
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DCreateRect(
                rect.X, rect.Y, rect.Width, rect.Height, out IntPtr nativeHandle));
            handle = NativeSubdiv2DHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Creates a subdivision for a floating-point reference rectangle. 为浮点参考矩形创建细分。</summary>
        public Subdiv2D(Rect2f rect)
        {
            ValidateRect(rect);
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DCreateRect2f(
                rect.X, rect.Y, rect.Width, rect.Height, out IntPtr nativeHandle));
            handle = NativeSubdiv2DHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether the native subdivision has been released. 获取原生细分是否已释放。</summary>
        public bool IsDisposed { get { return disposed; } }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Reinitializes the Delaunay subdivision. 重新初始化 Delaunay 细分。</summary>
        public void InitDelaunay(Rect rect)
        {
            ValidateRect(rect);
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DInitDelaunay(
                NativeHandle, rect.X, rect.Y, rect.Width, rect.Height));
        }

        /// <summary>Reinitializes the Delaunay subdivision with floating-point bounds. 使用浮点边界重新初始化 Delaunay 细分。</summary>
        public void InitDelaunay(Rect2f rect)
        {
            ValidateRect(rect);
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DInitDelaunayRect2f(
                NativeHandle, rect.X, rect.Y, rect.Width, rect.Height));
        }

        /// <summary>Inserts one point and returns its vertex identifier. 插入一个点并返回顶点标识。</summary>
        public int Insert(Point2f point)
        {
            ValidatePoint(point, nameof(point));
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DInsert(
                NativeHandle, point.X, point.Y, out int vertex));
            return vertex;
        }

        /// <summary>Inserts multiple points. 插入多个点。</summary>
        public void Insert(Point2f[] points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            var nativePoints = ToNativePoints(points);
            fixed (NativeMethods.Calib3DPoint2fNative* pointer = nativePoints)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DInsertPoints(
                    NativeHandle, pointer, nativePoints.Length));
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Inserts multiple points from a span. 从 Span 插入多个点。</summary>
        public void Insert(ReadOnlySpan<Point2f> points)
        {
            Insert(points.ToArray());
        }
#endif

        /// <summary>Locates a point and returns the associated edge and vertex identifiers. 定位点并返回相关边与顶点标识。</summary>
        public Subdiv2DPointLocation Locate(Point2f point, out int edge, out int vertex)
        {
            ValidatePoint(point, nameof(point));
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DLocate(
                NativeHandle, point.X, point.Y, out int location, out edge, out vertex));
            return (Subdiv2DPointLocation)location;
        }

        /// <summary>Finds the closest subdivision vertex. 查找最近的细分顶点。</summary>
        public int FindNearest(Point2f point, out Point2f nearestPoint)
        {
            ValidatePoint(point, nameof(point));
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DFindNearest(
                NativeHandle, point.X, point.Y, out int vertex, out float x, out float y));
            nearestPoint = new Point2f(x, y);
            return vertex;
        }

        /// <summary>Returns every geometric edge as origin and destination coordinates. 返回所有几何边的起点和终点坐标。</summary>
        public Vec4f[] GetEdgeList()
        {
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DGetEdgeListCount(NativeHandle, out int count));
            var nativeValues = new NativeMethods.Calib3DVec4fNative[count];
            fixed (NativeMethods.Calib3DVec4fNative* pointer = nativeValues)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DGetEdgeListFill(NativeHandle, pointer, count));
            }
            var result = new Vec4f[count];
            for (int index = 0; index < count; ++index)
            {
                NativeMethods.Calib3DVec4fNative value = nativeValues[index];
                result[index] = new Vec4f(value.V0, value.V1, value.V2, value.V3);
            }
            return result;
        }

        /// <summary>Returns one leading edge identifier for each triangle. 返回每个三角形的一条主边标识。</summary>
        public int[] GetLeadingEdgeList()
        {
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DGetLeadingEdgeListCount(NativeHandle, out int count));
            var result = new int[count];
            fixed (int* pointer = result)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DGetLeadingEdgeListFill(NativeHandle, pointer, count));
            }
            return result;
        }

        /// <summary>Returns every triangle as three two-dimensional vertices. 返回由三个二维顶点组成的全部三角形。</summary>
        public Vec6f[] GetTriangleList()
        {
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DGetTriangleListCount(NativeHandle, out int count));
            var nativeValues = new NativeMethods.Calib3DVec6fNative[count];
            fixed (NativeMethods.Calib3DVec6fNative* pointer = nativeValues)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DGetTriangleListFill(NativeHandle, pointer, count));
            }
            var result = new Vec6f[count];
            for (int index = 0; index < count; ++index)
            {
                NativeMethods.Calib3DVec6fNative value = nativeValues[index];
                result[index] = new Vec6f(value.V0, value.V1, value.V2, value.V3, value.V4, value.V5);
            }
            return result;
        }

        /// <summary>Returns Voronoi facets and their centers for selected vertices, or all vertices when indices is empty. 返回所选顶点的 Voronoi 面及中心；索引为空时返回全部。</summary>
        public void GetVoronoiFacetList(int[]? indices, out Point2f[][] facets, out Point2f[] centers)
        {
            indices = indices ?? Array.Empty<int>();
            fixed (int* indexPointer = indices)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DGetVoronoiFacetListCount(
                    NativeHandle, indexPointer, indices.Length, out int facetCount, out int pointCount));

                var offsets = new int[facetCount + 1];
                var nativePoints = new NativeMethods.Calib3DPoint2fNative[pointCount];
                var nativeCenters = new NativeMethods.Calib3DPoint2fNative[facetCount];
                fixed (int* offsetPointer = offsets)
                fixed (NativeMethods.Calib3DPoint2fNative* pointPointer = nativePoints)
                fixed (NativeMethods.Calib3DPoint2fNative* centerPointer = nativeCenters)
                {
                    NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DGetVoronoiFacetListFill(
                        NativeHandle,
                        indexPointer,
                        indices.Length,
                        offsetPointer,
                        offsets.Length,
                        pointPointer,
                        nativePoints.Length,
                        centerPointer,
                        nativeCenters.Length));
                }

                facets = new Point2f[facetCount][];
                centers = new Point2f[facetCount];
                for (int facet = 0; facet < facetCount; ++facet)
                {
                    int length = offsets[facet + 1] - offsets[facet];
                    facets[facet] = new Point2f[length];
                    for (int point = 0; point < length; ++point)
                    {
                        NativeMethods.Calib3DPoint2fNative value = nativePoints[offsets[facet] + point];
                        facets[facet][point] = new Point2f(value.X, value.Y);
                    }
                    centers[facet] = new Point2f(nativeCenters[facet].X, nativeCenters[facet].Y);
                }
            }
        }

        /// <summary>Returns a vertex location and its first connected edge. 返回顶点位置及其第一条相连边。</summary>
        public Point2f GetVertex(int vertex, out int firstEdge)
        {
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DGetVertex(
                NativeHandle, vertex, out float x, out float y, out firstEdge));
            return new Point2f(x, y);
        }

        /// <summary>Returns a related edge selected by quad-edge navigation. 按 quad-edge 导航返回相关边。</summary>
        public int GetEdge(int edge, Subdiv2DEdgeNavigation navigation)
        {
            if (!Enum.IsDefined(typeof(Subdiv2DEdgeNavigation), navigation))
                throw new ArgumentOutOfRangeException(nameof(navigation));
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DGetEdge(
                NativeHandle, edge, (int)navigation, out int relatedEdge));
            return relatedEdge;
        }

        /// <summary>Returns the next edge around the origin. 返回绕起点的下一条边。</summary>
        public int NextEdge(int edge)
        {
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DNextEdge(NativeHandle, edge, out int value));
            return value;
        }

        /// <summary>Returns another edge in the same quad-edge. 返回同一 quad-edge 中的另一条边。</summary>
        public int RotateEdge(int edge, int rotate)
        {
            if (rotate < 0 || rotate > 3) throw new ArgumentOutOfRangeException(nameof(rotate));
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DRotateEdge(NativeHandle, edge, rotate, out int value));
            return value;
        }

        /// <summary>Returns the symmetric edge. 返回对称边。</summary>
        public int SymEdge(int edge)
        {
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DSymEdge(NativeHandle, edge, out int value));
            return value;
        }

        /// <summary>Returns the edge origin vertex and location. 返回边的起点顶点及位置。</summary>
        public int EdgeOrg(int edge, out Point2f point)
        {
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DEdgeOrg(
                NativeHandle, edge, out int vertex, out float x, out float y));
            point = new Point2f(x, y);
            return vertex;
        }

        /// <summary>Returns the edge destination vertex and location. 返回边的终点顶点及位置。</summary>
        public int EdgeDst(int edge, out Point2f point)
        {
            NativeException.ThrowIfError(NativeMethods.Calib3DSubdiv2DEdgeDst(
                NativeHandle, edge, out int vertex, out float x, out float y));
            point = new Point2f(x, y);
            return vertex;
        }

        /// <summary>Releases the native subdivision. 释放原生细分。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        private static NativeMethods.Calib3DPoint2fNative[] ToNativePoints(Point2f[] points)
        {
            var result = new NativeMethods.Calib3DPoint2fNative[points.Length];
            for (int index = 0; index < points.Length; ++index)
            {
                ValidatePoint(points[index], nameof(points));
                result[index] = new NativeMethods.Calib3DPoint2fNative { X = points[index].X, Y = points[index].Y };
            }
            return result;
        }

        private static void ValidatePoint(Point2f point, string parameterName)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y))
                throw new ArgumentOutOfRangeException(parameterName, "Point coordinates must be finite.");
        }

        private static void ValidateRect(Rect rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                throw new ArgumentOutOfRangeException(nameof(rect), "Subdivision bounds must have positive dimensions.");
        }

        private static void ValidateRect(Rect2f rect)
        {
            if (float.IsNaN(rect.X) || float.IsInfinity(rect.X) ||
                float.IsNaN(rect.Y) || float.IsInfinity(rect.Y) ||
                float.IsNaN(rect.Width) || float.IsInfinity(rect.Width) ||
                float.IsNaN(rect.Height) || float.IsInfinity(rect.Height) ||
                rect.Width <= 0.0F || rect.Height <= 0.0F)
                throw new ArgumentOutOfRangeException(nameof(rect), "Subdivision bounds must be finite with positive dimensions.");
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
