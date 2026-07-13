using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;

namespace OpenCvSharp.Tests.Calib3D
{
    internal static class CalibrationTestData
    {
        internal static readonly Size ImageSize = new Size(640, 480);

        internal static void CreateSyntheticCalibrationData(
            out Point3f[][] objectPoints,
            out Point2f[][] imagePoints)
        {
            const int boardRows = 6;
            const int boardColumns = 7;
            const int viewCount = 10;
            const double squareSize = 0.04;
            const double fx = 800.0;
            const double fy = 820.0;
            const double cx = 320.0;
            const double cy = 240.0;

            var board = new Point3f[boardRows * boardColumns];
            int pointIndex = 0;
            for (int row = 0; row < boardRows; ++row)
            {
                for (int column = 0; column < boardColumns; ++column)
                {
                    board[pointIndex++] = new Point3f(
                        (float)((column - 3.0) * squareSize),
                        (float)((row - 2.5) * squareSize),
                        0.0F);
                }
            }

            objectPoints = new Point3f[viewCount][];
            imagePoints = new Point2f[viewCount][];
            for (int view = 0; view < viewCount; ++view)
            {
                objectPoints[view] = (Point3f[])board.Clone();
                imagePoints[view] = new Point2f[board.Length];

                double angleX = -0.16 + view * 0.035;
                double angleY = 0.13 - view * 0.021;
                double angleZ = -0.08 + view * 0.018;
                double translationX = -0.08 + view * 0.017;
                double translationY = 0.05 - view * 0.011;
                double translationZ = 0.95 + view * 0.045;
                ComputeRotationMatrix(angleX, angleY, angleZ, out double[] rotation);

                for (int point = 0; point < board.Length; ++point)
                {
                    double x = board[point].X;
                    double y = board[point].Y;
                    double z = board[point].Z;
                    double cameraX = rotation[0] * x + rotation[1] * y + rotation[2] * z + translationX;
                    double cameraY = rotation[3] * x + rotation[4] * y + rotation[5] * z + translationY;
                    double cameraZ = rotation[6] * x + rotation[7] * y + rotation[8] * z + translationZ;
                    double deterministicNoise = ((point + view * 3) % 7 - 3) * 0.002;

                    imagePoints[view][point] = new Point2f(
                        (float)(fx * cameraX / cameraZ + cx + deterministicNoise),
                        (float)(fy * cameraY / cameraZ + cy - deterministicNoise));
                }
            }
        }

        internal static void CreateSyntheticCameraRegistrationData(
            out Point3f[][] objectPoints1,
            out Point3f[][] objectPoints2,
            out Point2f[][] imagePoints1,
            out Point2f[][] imagePoints2,
            out Mat cameraMatrix1,
            out Mat distCoeffs1,
            out Mat cameraMatrix2,
            out Mat distCoeffs2)
        {
            const int boardRows = 6;
            const int boardColumns = 7;
            const int viewCount = 10;
            const double squareSize = 0.04;
            const double fx1 = 800.0;
            const double fy1 = 820.0;
            const double cx1 = 320.0;
            const double cy1 = 240.0;
            const double fx2 = 790.0;
            const double fy2 = 805.0;
            const double cx2 = 315.0;
            const double cy2 = 238.0;

            var board = new Point3f[boardRows * boardColumns];
            int pointIndex = 0;
            for (int row = 0; row < boardRows; ++row)
            {
                for (int column = 0; column < boardColumns; ++column)
                {
                    board[pointIndex++] = new Point3f(
                        (float)((column - 3.0) * squareSize),
                        (float)((row - 2.5) * squareSize),
                        0.0F);
                }
            }

            ComputeRotationMatrix(0.018, -0.052, 0.027, out double[] camera2FromCamera1);
            double[] camera2Translation = { 0.18, -0.012, 0.024 };

            objectPoints1 = new Point3f[viewCount][];
            objectPoints2 = new Point3f[viewCount][];
            imagePoints1 = new Point2f[viewCount][];
            imagePoints2 = new Point2f[viewCount][];
            for (int view = 0; view < viewCount; ++view)
            {
                objectPoints1[view] = (Point3f[])board.Clone();
                objectPoints2[view] = (Point3f[])board.Clone();

                double angleX = -0.15 + view * 0.031;
                double angleY = 0.12 - view * 0.019;
                double angleZ = -0.07 + view * 0.016;
                ComputeRotationMatrix(angleX, angleY, angleZ, out double[] boardToCamera1);
                double[] boardTranslation1 =
                {
                    -0.075 + view * 0.016,
                    0.045 - view * 0.009,
                    1.05 + view * 0.04
                };

                double[] boardToCamera2 = MultiplyRotation(camera2FromCamera1, boardToCamera1);
                double[] boardTranslation2 = TransformTranslation(
                    camera2FromCamera1,
                    boardTranslation1,
                    camera2Translation);

                imagePoints1[view] = ProjectPinhole(
                    board,
                    boardToCamera1,
                    boardTranslation1,
                    fx1,
                    fy1,
                    cx1,
                    cy1,
                    view);
                imagePoints2[view] = ProjectPinhole(
                    board,
                    boardToCamera2,
                    boardTranslation2,
                    fx2,
                    fy2,
                    cx2,
                    cy2,
                    view + 3);
            }

            cameraMatrix1 = CreateCameraMatrix(fx1, fy1, cx1, cy1);
            cameraMatrix2 = CreateCameraMatrix(fx2, fy2, cx2, cy2);
            distCoeffs1 = new Mat(1, 5, MatType.CV_64FC1, new Scalar(0.0));
            distCoeffs2 = new Mat(1, 5, MatType.CV_64FC1, new Scalar(0.0));
        }

        internal static void CreateSyntheticMultiviewCalibrationData(
            out Point3f[][] objectPoints,
            out Point2f[][][] imagePoints,
            out Size[] imageSizes,
            out bool[][] detectionMask,
            out CameraModel[] cameraModels)
        {
            const int boardRows = 6;
            const int boardColumns = 7;
            const int cameraCount = 3;
            const int frameCount = 16;
            const double squareSize = 0.04;

            var board = new Point3f[boardRows * boardColumns];
            int pointIndex = 0;
            for (int row = 0; row < boardRows; ++row)
            {
                for (int column = 0; column < boardColumns; ++column)
                {
                    board[pointIndex++] = new Point3f(
                        (float)((column - 3.0) * squareSize),
                        (float)((row - 2.5) * squareSize),
                        0.0F);
                }
            }

            double[] focalX = { 800.0, 790.0, 820.0 };
            double[] focalY = { 815.0, 805.0, 810.0 };
            double[] centerX = { 320.0, 315.0, 322.0 };
            double[] centerY = { 240.0, 238.0, 242.0 };

            ComputeRotationMatrix(0.0, 0.0, 0.0, out double[] camera0FromCamera0);
            ComputeRotationMatrix(0.015, -0.045, 0.020, out double[] camera1FromCamera0);
            ComputeRotationMatrix(-0.010, -0.082, 0.035, out double[] camera2FromCamera0);
            double[][] cameraRotations =
            {
                camera0FromCamera0,
                camera1FromCamera0,
                camera2FromCamera0
            };
            double[][] cameraTranslations =
            {
                new[] { 0.0, 0.0, 0.0 },
                new[] { 0.18, -0.012, 0.025 },
                new[] { 0.36, 0.015, 0.04 }
            };

            detectionMask = new bool[cameraCount][];
            for (int camera = 0; camera < cameraCount; ++camera)
            {
                detectionMask[camera] = new bool[frameCount];
            }
            for (int frame = 0; frame < frameCount; ++frame)
            {
                detectionMask[0][frame] = frame <= 9;
                detectionMask[1][frame] = frame >= 5 && frame <= 13;
                detectionMask[2][frame] = frame >= 10 && frame <= 14;
            }

            objectPoints = new Point3f[frameCount][];
            imagePoints = new Point2f[cameraCount][][];
            for (int camera = 0; camera < cameraCount; ++camera)
            {
                imagePoints[camera] = new Point2f[frameCount][];
            }

            for (int frame = 0; frame < frameCount; ++frame)
            {
                objectPoints[frame] = (Point3f[])board.Clone();

                double angleX = -0.18 + frame * 0.021;
                double angleY = 0.16 - frame * 0.015;
                double angleZ = -0.10 + frame * 0.012;
                ComputeRotationMatrix(angleX, angleY, angleZ, out double[] boardToCamera0);
                double[] boardTranslation0 =
                {
                    -0.12 + frame * 0.015,
                    0.07 - frame * 0.008,
                    1.05 + frame * 0.035
                };

                for (int camera = 0; camera < cameraCount; ++camera)
                {
                    if (!detectionMask[camera][frame])
                    {
                        imagePoints[camera][frame] = Array.Empty<Point2f>();
                        continue;
                    }

                    double[] boardToCamera =
                        MultiplyRotation(cameraRotations[camera], boardToCamera0);
                    double[] boardTranslation = TransformTranslation(
                        cameraRotations[camera],
                        boardTranslation0,
                        cameraTranslations[camera]);
                    imagePoints[camera][frame] = ProjectPinhole(
                        board,
                        boardToCamera,
                        boardTranslation,
                        focalX[camera],
                        focalY[camera],
                        centerX[camera],
                        centerY[camera],
                        frame + camera * 5);
                }
            }

            imageSizes = new[] { ImageSize, ImageSize, ImageSize };
            cameraModels = new[]
            {
                CameraModel.Pinhole,
                CameraModel.Pinhole,
                CameraModel.Pinhole
            };
        }

        internal static void CreateSyntheticFisheyeCalibrationData(
            out Point3f[][] objectPoints,
            out Point2f[][] imagePoints,
            out Mat cameraMatrix,
            out Mat distCoeffs)
        {
            CreateSyntheticFisheyeStereoCalibrationData(
                out objectPoints,
                out imagePoints,
                out _,
                out cameraMatrix,
                out distCoeffs,
                out Mat cameraMatrix2,
                out Mat distCoeffs2);
            cameraMatrix2.Dispose();
            distCoeffs2.Dispose();
        }

        internal static void CreateSyntheticFisheyePoseData(
            out Point3f[] objectPoints,
            out Point2f[] imagePoints,
            out Mat cameraMatrix,
            out Mat distCoeffs,
            out Mat rvec,
            out Mat tvec)
        {
            const double fx = 430.0;
            const double fy = 425.0;
            const double cx = 320.0;
            const double cy = 240.0;
            double[] distortion = { -0.018, 0.006, -0.0015, 0.00025 };
            double[] rotationVector = { 0.16, -0.11, 0.07 };
            double[] translation = { 0.04, -0.03, 1.35 };

            objectPoints = new Point3f[30];
            int pointIndex = 0;
            for (int layer = 0; layer < 2; ++layer)
            {
                for (int row = 0; row < 3; ++row)
                {
                    for (int column = 0; column < 5; ++column)
                    {
                        objectPoints[pointIndex++] = new Point3f(
                            (float)((column - 2.0) * 0.12),
                            (float)((row - 1.0) * 0.11),
                            (float)((layer - 0.5) * 0.16 + ((row + column) % 2) * 0.025));
                    }
                }
            }

            ComputeRodriguesRotation(rotationVector, out double[] rotation);
            imagePoints = ProjectFisheye(
                objectPoints,
                rotation,
                translation,
                fx,
                fy,
                cx,
                cy,
                distortion,
                0,
                0.0);

            cameraMatrix = CreateCameraMatrix(fx, fy, cx, cy);
            distCoeffs = CreateFisheyeDistCoeffs(distortion);
            rvec = new Mat(3, 1, MatType.CV_64FC1, new Scalar(0.0));
            tvec = new Mat(3, 1, MatType.CV_64FC1, new Scalar(0.0));
            for (int i = 0; i < 3; ++i)
            {
                rvec.SetValue(i, rotationVector[i]);
                tvec.SetValue(i, translation[i]);
            }
        }

        internal static void CreateSyntheticFisheyeStereoCalibrationData(
            out Point3f[][] objectPoints,
            out Point2f[][] imagePoints1,
            out Point2f[][] imagePoints2,
            out Mat cameraMatrix1,
            out Mat distCoeffs1,
            out Mat cameraMatrix2,
            out Mat distCoeffs2)
        {
            const int boardRows = 6;
            const int boardColumns = 7;
            const int viewCount = 14;
            const double squareSize = 0.04;
            const double fx1 = 430.0;
            const double fy1 = 425.0;
            const double cx1 = 320.0;
            const double cy1 = 240.0;
            const double fx2 = 438.0;
            const double fy2 = 432.0;
            const double cx2 = 317.0;
            const double cy2 = 242.0;
            double[] distortion1 = { -0.018, 0.006, -0.0015, 0.00025 };
            double[] distortion2 = { -0.014, 0.0045, -0.0011, 0.00018 };

            var board = new Point3f[boardRows * boardColumns];
            int pointIndex = 0;
            for (int row = 0; row < boardRows; ++row)
            {
                for (int column = 0; column < boardColumns; ++column)
                {
                    board[pointIndex++] = new Point3f(
                        (float)((column - 3.0) * squareSize),
                        (float)((row - 2.5) * squareSize),
                        0.0F);
                }
            }

            ComputeRotationMatrix(0.014, -0.047, 0.021, out double[] camera2FromCamera1);
            double[] camera2Translation = { 0.16, -0.008, 0.014 };

            objectPoints = new Point3f[viewCount][];
            imagePoints1 = new Point2f[viewCount][];
            imagePoints2 = new Point2f[viewCount][];
            for (int view = 0; view < viewCount; ++view)
            {
                objectPoints[view] = (Point3f[])board.Clone();

                double angleX = -0.25 + view * 0.038;
                double angleY = 0.21 - view * 0.031;
                double angleZ = -0.13 + view * 0.021;
                ComputeRotationMatrix(angleX, angleY, angleZ, out double[] boardToCamera1);
                double[] boardTranslation1 =
                {
                    -0.115 + view * 0.017,
                    0.072 - view * 0.010,
                    0.82 + view * 0.035
                };

                double[] boardToCamera2 = MultiplyRotation(camera2FromCamera1, boardToCamera1);
                double[] boardTranslation2 = TransformTranslation(
                    camera2FromCamera1,
                    boardTranslation1,
                    camera2Translation);

                imagePoints1[view] = ProjectFisheye(
                    board,
                    boardToCamera1,
                    boardTranslation1,
                    fx1,
                    fy1,
                    cx1,
                    cy1,
                    distortion1,
                    view);
                imagePoints2[view] = ProjectFisheye(
                    board,
                    boardToCamera2,
                    boardTranslation2,
                    fx2,
                    fy2,
                    cx2,
                    cy2,
                    distortion2,
                    view + 5);
            }

            cameraMatrix1 = CreateCameraMatrix(fx1, fy1, cx1, cy1);
            distCoeffs1 = CreateFisheyeDistCoeffs(distortion1);
            cameraMatrix2 = CreateCameraMatrix(fx2, fy2, cx2, cy2);
            distCoeffs2 = CreateFisheyeDistCoeffs(distortion2);
        }

        private static Mat CreateCameraMatrix(double fx, double fy, double cx, double cy)
        {
            var cameraMatrix = new Mat(3, 3, MatType.CV_64FC1, new Scalar(0.0));
            cameraMatrix.SetValue(0, fx);
            cameraMatrix.SetValue(2, cx);
            cameraMatrix.SetValue(4, fy);
            cameraMatrix.SetValue(5, cy);
            cameraMatrix.SetValue(8, 1.0);
            return cameraMatrix;
        }

        private static Mat CreateFisheyeDistCoeffs(double[] values)
        {
            var distCoeffs = new Mat(4, 1, MatType.CV_64FC1, new Scalar(0.0));
            for (int i = 0; i < values.Length; ++i)
            {
                distCoeffs.SetValue(i, values[i]);
            }
            return distCoeffs;
        }

        private static Point2f[] ProjectFisheye(
            Point3f[] objectPoints,
            double[] rotation,
            double[] translation,
            double fx,
            double fy,
            double cx,
            double cy,
            double[] distortion,
            int noisePhase,
            double noiseScale = 0.0002)
        {
            var imagePoints = new Point2f[objectPoints.Length];
            for (int point = 0; point < objectPoints.Length; ++point)
            {
                double x = objectPoints[point].X;
                double y = objectPoints[point].Y;
                double z = objectPoints[point].Z;
                double cameraX = rotation[0] * x + rotation[1] * y + rotation[2] * z + translation[0];
                double cameraY = rotation[3] * x + rotation[4] * y + rotation[5] * z + translation[1];
                double cameraZ = rotation[6] * x + rotation[7] * y + rotation[8] * z + translation[2];
                double normalizedX = cameraX / cameraZ;
                double normalizedY = cameraY / cameraZ;
                double radius = Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
                double theta = Math.Atan(radius);
                double theta2 = theta * theta;
                double theta4 = theta2 * theta2;
                double theta6 = theta4 * theta2;
                double theta8 = theta4 * theta4;
                double distortedTheta = theta * (
                    1.0 +
                    distortion[0] * theta2 +
                    distortion[1] * theta4 +
                    distortion[2] * theta6 +
                    distortion[3] * theta8);
                double scale = radius > 1.0e-12 ? distortedTheta / radius : 1.0;
                double deterministicNoise =
                    ((point * 3 + noisePhase * 5) % 11 - 5) * noiseScale;

                imagePoints[point] = new Point2f(
                    (float)(fx * normalizedX * scale + cx + deterministicNoise),
                    (float)(fy * normalizedY * scale + cy - deterministicNoise));
            }
            return imagePoints;
        }

        private static Point2f[] ProjectPinhole(
            Point3f[] objectPoints,
            double[] rotation,
            double[] translation,
            double fx,
            double fy,
            double cx,
            double cy,
            int noisePhase)
        {
            var imagePoints = new Point2f[objectPoints.Length];
            for (int point = 0; point < objectPoints.Length; ++point)
            {
                double x = objectPoints[point].X;
                double y = objectPoints[point].Y;
                double z = objectPoints[point].Z;
                double cameraX = rotation[0] * x + rotation[1] * y + rotation[2] * z + translation[0];
                double cameraY = rotation[3] * x + rotation[4] * y + rotation[5] * z + translation[1];
                double cameraZ = rotation[6] * x + rotation[7] * y + rotation[8] * z + translation[2];
                double deterministicNoise = ((point * 2 + noisePhase * 3) % 9 - 4) * 0.001;

                imagePoints[point] = new Point2f(
                    (float)(fx * cameraX / cameraZ + cx + deterministicNoise),
                    (float)(fy * cameraY / cameraZ + cy - deterministicNoise));
            }
            return imagePoints;
        }

        private static double[] MultiplyRotation(double[] left, double[] right)
        {
            var result = new double[9];
            for (int row = 0; row < 3; ++row)
            {
                for (int column = 0; column < 3; ++column)
                {
                    result[row * 3 + column] =
                        left[row * 3] * right[column] +
                        left[row * 3 + 1] * right[3 + column] +
                        left[row * 3 + 2] * right[6 + column];
                }
            }
            return result;
        }

        private static double[] TransformTranslation(
            double[] rotation,
            double[] source,
            double[] translation)
        {
            return new[]
            {
                rotation[0] * source[0] + rotation[1] * source[1] + rotation[2] * source[2] + translation[0],
                rotation[3] * source[0] + rotation[4] * source[1] + rotation[5] * source[2] + translation[1],
                rotation[6] * source[0] + rotation[7] * source[1] + rotation[8] * source[2] + translation[2]
            };
        }

        private static void ComputeRotationMatrix(
            double angleX,
            double angleY,
            double angleZ,
            out double[] rotation)
        {
            double cx = Math.Cos(angleX);
            double sx = Math.Sin(angleX);
            double cy = Math.Cos(angleY);
            double sy = Math.Sin(angleY);
            double cz = Math.Cos(angleZ);
            double sz = Math.Sin(angleZ);

            rotation = new[]
            {
                cz * cy,
                cz * sy * sx - sz * cx,
                cz * sy * cx + sz * sx,
                sz * cy,
                sz * sy * sx + cz * cx,
                sz * sy * cx - cz * sx,
                -sy,
                cy * sx,
                cy * cx
            };
        }

        private static void ComputeRodriguesRotation(double[] rotationVector, out double[] rotation)
        {
            double x = rotationVector[0];
            double y = rotationVector[1];
            double z = rotationVector[2];
            double theta = Math.Sqrt(x * x + y * y + z * z);
            if (theta <= 1.0e-15)
            {
                rotation = new[]
                {
                    1.0, 0.0, 0.0,
                    0.0, 1.0, 0.0,
                    0.0, 0.0, 1.0
                };
                return;
            }

            double axisX = x / theta;
            double axisY = y / theta;
            double axisZ = z / theta;
            double cosine = Math.Cos(theta);
            double sine = Math.Sin(theta);
            double oneMinusCosine = 1.0 - cosine;

            rotation = new[]
            {
                cosine + axisX * axisX * oneMinusCosine,
                axisX * axisY * oneMinusCosine - axisZ * sine,
                axisX * axisZ * oneMinusCosine + axisY * sine,
                axisY * axisX * oneMinusCosine + axisZ * sine,
                cosine + axisY * axisY * oneMinusCosine,
                axisY * axisZ * oneMinusCosine - axisX * sine,
                axisZ * axisX * oneMinusCosine - axisY * sine,
                axisZ * axisY * oneMinusCosine + axisX * sine,
                cosine + axisZ * axisZ * oneMinusCosine
            };
        }
    }
}
