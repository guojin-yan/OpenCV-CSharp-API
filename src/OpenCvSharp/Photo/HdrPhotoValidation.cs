using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Photo
{
    internal static class HdrPhotoValidation
    {
        internal static IntPtr[] GetImageHandles(
            Mat[] images,
            string parameterName,
            bool requireEightBit,
            bool allowHighDynamicRangeDepth,
            bool requireColor,
            bool allowFourChannels,
            bool allowAnyDepth)
        {
            if (images == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            if (images.Length == 0)
            {
                throw new ArgumentException("Image collection cannot be empty.", parameterName);
            }

            var handles = new IntPtr[images.Length];
            Mat first = RequireMat(images[0], parameterName);
            if (first.Empty)
            {
                throw new ArgumentException("Image collection cannot contain empty matrices.", parameterName);
            }

            ValidateImageType(
                first,
                parameterName,
                requireEightBit,
                allowHighDynamicRangeDepth,
                requireColor,
                allowFourChannels,
                allowAnyDepth);
            handles[0] = first.NativeHandle;
            for (int i = 1; i < images.Length; ++i)
            {
                Mat image = RequireMat(images[i], parameterName);
                if (image.Empty)
                {
                    throw new ArgumentException("Image collection cannot contain empty matrices.", parameterName);
                }
                if (image.Rows != first.Rows || image.Cols != first.Cols || image.Type != first.Type)
                {
                    throw new ArgumentException("All input images must have the same size and type.", parameterName);
                }
                handles[i] = image.NativeHandle;
            }
            return handles;
        }

        internal static IntPtr[] GetOutputHandles(Mat[] output, int expectedCount, string parameterName)
        {
            if (output == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            if (output.Length != expectedCount)
            {
                throw new ArgumentException("Output image count must match the input image count.", parameterName);
            }

            var handles = new IntPtr[output.Length];
            for (int i = 0; i < output.Length; ++i)
            {
                handles[i] = RequireMat(output[i], parameterName).NativeHandle;
            }
            return handles;
        }

        internal static Mat[] CreateOutputMats(int count)
        {
            var result = new Mat[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = new Mat();
            }
            return result;
        }

        internal static void DisposeAll(Mat[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                values[i]?.Dispose();
            }
        }

        internal static void ValidateTimes(Mat times, int imageCount, string parameterName)
        {
            RequireMat(times, parameterName);
            if (times.Empty || times.Type != MatType.CV_32FC1 || times.Total.ToUInt64() != (ulong)imageCount)
            {
                throw new ArgumentException(
                    "Exposure times must be a non-empty CV_32FC1 matrix with one value per image.",
                    parameterName);
            }
        }

        internal static void ValidateResponse(Mat response, string parameterName)
        {
            RequireMat(response, parameterName);
            if (!response.Empty && (response.Depth != MatType.CV_32F || response.Cols != 1))
            {
                throw new ArgumentException(
                    "Camera response must be empty or a single-column CV_32F matrix.",
                    parameterName);
            }
        }

        internal static Mat RequireMat(Mat value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }

        internal static void ValidateSingleChannelByte(Mat value, string parameterName)
        {
            RequireMat(value, parameterName);
            if (value.Empty || value.Type != MatType.CV_8UC1)
            {
                throw new ArgumentException("Matrix must be a non-empty CV_8UC1 image.", parameterName);
            }
        }

        internal static void ValidateDistinct(Mat first, Mat second, string parameterName)
        {
            if (ReferenceEquals(first, second))
            {
                throw new ArgumentException("Input and output matrices must be distinct.", parameterName);
            }
        }

        private static void ValidateImageType(
            Mat image,
            string parameterName,
            bool requireEightBit,
            bool allowHighDynamicRangeDepth,
            bool requireColor,
            bool allowFourChannels,
            bool allowAnyDepth)
        {
            bool validChannels = requireColor
                ? image.Channels == 3 || (allowFourChannels && image.Channels == 4)
                : image.Channels == 1 || image.Channels == 3;
            if (!validChannels)
            {
                throw new ArgumentException(
                    requireColor
                        ? "Alignment images must have three or four channels."
                        : "HDR images must have one or three channels.",
                    parameterName);
            }
            if (!allowAnyDepth && requireEightBit && image.Depth != MatType.CV_8U)
            {
                throw new ArgumentException("Calibration and alignment images must have CV_8U depth.", parameterName);
            }
            if (!allowAnyDepth && allowHighDynamicRangeDepth &&
                image.Depth != MatType.CV_8U &&
                image.Depth != MatType.CV_16U &&
                image.Depth != MatType.CV_32F)
            {
                throw new ArgumentException("Merge images must have CV_8U, CV_16U, or CV_32F depth.", parameterName);
            }
        }
    }
}
