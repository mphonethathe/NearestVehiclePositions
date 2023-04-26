using System;
using System.Collections.Generic;
using System.IO;

namespace NearestVehiclePositions
{
    class Program
    {
        public static void Main(string[] args)
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

            if (vehicles == null)
            {
                throw new ArgumentException("no vehicles found");
            }

            // Find nearest vehicle position to each coordinate
            foreach (var coordinate in coordinates)
            {
                VehiclePosition nearestVehicle = null;
                double minDistance = double.MaxValue;

                foreach (var vehicle in vehicles)
                {
                    var distance = CalculateDistance(coordinate.latitude, coordinate.longitude, vehicle.Latitude,
                        vehicle.Longitude);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestVehicle = vehicle;
                    }
                }

                Console.WriteLine(
                    $"Nearest vehicle to ({coordinate.latitude}, {coordinate.longitude}) is {nearestVehicle.VehicleRegistration} at ({nearestVehicle.Latitude}, {nearestVehicle.Longitude})");
            }
        }

        static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371; // Earth's radius in kilometers
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = r * c;

            return distance;
        }

        static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
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
                            PositionId = positionId,
                            VehicleRegistration = vehicleRegistration,
                            Latitude = latitude,
                            Longitude = longitude,
                            RecordedTimeUtc = recordedTimeUtc
                        });
                    }
                }

                return positions;
            }
            catch (Exception Ex)
            {
                Console.WriteLine($"Error: {Ex.Message}");
            }


            return null;
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


        class VehiclePosition
        {
            public int PositionId;
            public string VehicleRegistration;
            public double Latitude;
            public double Longitude;
            public ulong RecordedTimeUtc;
        }
    }
}




