using System;
using System.Collections.Generic;
using System.IO;
using KdTree;
using KdTree.Math;

namespace NearestVehiclePositions
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load vehicle positions from binary file
            var vehicles = LoadVehiclePositions();

            // Coordinates to find the nearest vehicle positions to
            var coordinates = new List<(double latitude, double longitude)>
            {
                (34.544909, -102.100843),
                (32.345544, -99.123124),
                (33.234235, -100.214124),
                (35.195739, -95.348899),
                (31.895839, -97.789573),
                (32.895839, -101.789573),
                (34.115839, -100.225732),
                (32.335839, -99.992232),
                (33.535339, -94.792232),
                (32.234235, -100.222222)
            };

            // Build k-d tree of vehicle positions
            var tree = new KdTree<double, int>(2, new DoubleMath());

            if (vehicles == null)
            {
                throw new ArgumentException("no vehicles found");
            }


            foreach (var vehicle in vehicles)
            {
                tree.Add(new[] { vehicle.longitude, vehicle.latitude }, vehicle.positionId);
            }

            // Find nearest vehicle position to each coordinate
        

            Parallel.ForEach(coordinates, coordinate =>
            {

                var nearest = tree.GetNearestNeighbours(new[] { coordinate.longitude, coordinate.latitude }, 1)[0];
                var nearestVehicle = vehicles[nearest.Value];
                Console.WriteLine($"Nearest vehicle to ({coordinate.latitude}, {coordinate.longitude}) is {nearestVehicle.vehicleRegistration} at ({nearestVehicle.latitude}, {nearestVehicle.longitude})");
            });


        }

        static List<VehiclePosition> LoadVehiclePositions()
        {
            try
            {
                var positions = new List<VehiclePosition>();

                var inputFilePath = @"C:\Users\MphoNethathe\Desktop\Data\VehiclePositions.DAT";

                using (var file = new FileStream(inputFilePath, FileMode.Open))
                using (var reader = new BinaryReader(file))
                {
                    while (file.Position < file.Length)
                    {
                        var positionId = reader.ReadInt32();
                        var vehicleRegistration = ReadNullTerminatedString(reader);
                        var latitude = reader.ReadSingle();
                        var longitude = reader.ReadSingle();
                        var recordedTimeUtc = reader.ReadUInt64();

                        positions.Add(new VehiclePosition
                        {
                            positionId = positionId,
                            vehicleRegistration = vehicleRegistration,
                            latitude = latitude,
                            longitude = longitude,
                            recordedTimeUtc = recordedTimeUtc
                        });
                    }
                }

                return positions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.Message} ");
            }

            return  null;
        }

        static string ReadNullTerminatedString(BinaryReader reader)
        {
            try
            {
                var bytes = new List<byte>();
                byte b;
                while ((b = reader.ReadByte()) != 0)
                {
                    bytes.Add(b);
                }

                return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("error");
            }
        }
    }

    class VehiclePosition
    {
        public int positionId;
        public string vehicleRegistration;
        public double latitude;
        public double longitude;
        public ulong recordedTimeUtc;
    }
}
