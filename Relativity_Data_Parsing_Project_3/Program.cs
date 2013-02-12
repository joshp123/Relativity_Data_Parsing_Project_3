﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using System.Diagnostics;
// for timer

using System.Windows.Forms;
// used for MsgBox alerting to exception

using System.Text.RegularExpressions;
// used for parsing the file
/* 
 * A wise man once said:
 * "When you have a problem, and think 'I know, I'll solve it with regex',
 * well, now you have two problems"
 */

namespace Relativity_Data_Parsing_Project_3
{
    class Program
    {
        // Declaring universal physical constants at Class level.
        
        /// <summary>
        /// Speed of Light. Units of meters per second.
        /// </summary>
        const int speedOfLight = 299792458;

        /// <summary>
        /// Rest mass of positron. Units of MeV/c^2
        /// </summary>
        const double positronRestMass = 0.5109989;

        /// <summary>
        /// Event structure
        /// </summary>
        /// <remarks>
        /// Contains fields for times and energy
        /// TODO: (optional) measure performance of class vs struct
        /// </remarks>
        public struct Event
        {
            private const int detectorSeparation = 3;
   
            /// <summary>
            /// Time when particle detected at first detector. Units of nanoseconds
            /// </summary>
            public double time_1;
                        
            /// <summary>
            /// Time when particle detected at second detector. Units of nanoseconds
            /// </summary>
            public double time_2;

            /// <summary>
            /// Kinetic energy of decayed particle (positron). Units of MeV
            /// </summary>
            /// <remarks>
            /// This energy is a measured one.
            /// </remarks>
            public double energy;

            /// <summary>
            /// Total ernergy of the decayed particle (here, positron)
            /// </summary>
            /// <returns>Returns the total (kinetic energy + mass energy) energy of the particle that decayed</returns>
            public double TotalEnergy()
            {
                return this.energy + positronRestMass;
            }

            /// <summary>
            /// Get the velocity of the detected (pre-decay) particle in this event as a proportion of the speed of light
            /// </summary>
            /// <returns>Returns the velocity of the particle (unitless, as a fraction of c)</returns>
            public double Beta()
            {
                double velocity = detectorSeparation / ((this.time_2 - this.time_1) * Math.Pow(10, -9));
                return velocity / speedOfLight;
            }

            /// <summary>
            /// Get the Lorentz factor for the detected particle
            /// </summary>
            /// <returns>
            /// Returns the Lorentz factor for the detected particle in this event</returns>
            public double Gamma()
            {
                return 1 / (Math.Sqrt(1 - Math.Pow(this.Beta(), 2)));
            }

            /// <summary>
            /// The lifetime of the pre-decay particle in it's own reference frame
            /// </summary>
            /// <returns></returns>
            public double ParticleLiftetime()
            {
                return this.time_2 / this.Gamma();
            }

            /// <summary>
            /// Relativistic momentum of decayed particle. Units of MeV (it is multiplied by C)
            /// </summary>
            /// <returns>Returns the momentum of the decayed particle in the lab frame multiplied by c</returns>
            public double Momentum()
            {
                return Math.Sqrt(Math.Pow(this.TotalEnergy(), 2) - Math.Pow(positronRestMass, 2));
                // Square root of [ (energy^2) - m_0^2) ]
            }

            /// <summary>
            /// Calculate the total energy of the decayed particle (here, positron), in its own frame of reference
            /// </summary>
            /// <returns></returns>
            public double TransformEnergy()
            {
                return this.Gamma() * (this.TotalEnergy() - (this.Momentum() * this.Beta()));
                // TODO: make this actually give the correct value
            }

        }

        
        
        static void Main(string[] args)
        {
            string[] filepaths = new string[] {@"C:\Users\Josh\Downloads\p3data7.dat", @"C:\Users\Josh\Downloads\p3data100.dat",
                                               @"C:\Users\Josh\Downloads\p3data1M.dat", @"C:\Users\Josh\Downloads\p3data10M.dat"};
            string filename = Path.GetFileName(filepaths.Last());

            Stopwatch sw = new Stopwatch();

            sw.Start();

            List<Event> events = new List<Event>();

            foreach (var file in filepaths)
            {
                events.AddRange(ParseFile(file));
            }
            
            sw.Stop();

            Console.WriteLine("File read completed, {0} events parsed in {1} seconds", events.Count, sw.ElapsedMilliseconds.ToString());


            // take some quick data on the events

            var goodEvents = (from item in events
                              where item.energy >= 0
                              select item)
                              .ToList();
            // get the positive energy events, store in a new List goodEvents

            var badEvents = from item in events
                            where item.energy < 0
                            select item;

            int numberOfBadEvents = badEvents.Count();

            double averageEvergy = (from item in goodEvents select item.energy).Average();

            double averageT2 = (from item in goodEvents select item.time_2).Average();
            Console.WriteLine("\nStep 1 Calculations: ");
            Console.WriteLine("Number of bad events \t=" + numberOfBadEvents);
            Console.WriteLine("Average kinetic energy \t= " + averageEvergy + " MeV");
            Console.WriteLine("Average t2 \t\t=  " + averageT2 + " ns");

            //Console.WriteLine("Transforming Event [0]");
            //Console.WriteLine("Beta, Gamma, Momentum, Energy Transform (MeV), Particle Lifetime (ns)");
            //Console.WriteLine("{0}, {1}, {2}, {3}, {4}", events[0].Beta(), events[0].Gamma(), events[0].Momentum(), events[0].TransformEnergy(), events[0].ParticleLiftetime());

            var transformedEnergies = (from item in goodEvents
                                       select item.TransformEnergy())
                                      .ToList();

            var lifetimes = (from item in goodEvents
                             select item.ParticleLiftetime())
                             .ToList();

            // calculate some stats

            var largestTransformedEnergy = transformedEnergies.Max();
            var longestParticleLifetime = lifetimes.Max();

            var averageTransformedEnergy = transformedEnergies.Average();
            var averageParticleLifetime = lifetimes.Average();
            Console.WriteLine("\nStep 2 Calculations: ");
            Console.WriteLine("Largest transformed energy \t= " + largestTransformedEnergy + " MeV");
            Console.WriteLine("Longest particle lifetime \t= " + longestParticleLifetime + " ns");
            Console.WriteLine("Average transformed energy \t= " + averageTransformedEnergy + " MeV");
            Console.WriteLine("Average particle lifetime \t= " + averageParticleLifetime + " ns");

            // TODO: i mean i could write these to a separate file isntead of the console but it's
            // kinda pointless having a 4 line text file with this data but it's also stupid to
            // stick it in a .csv file and deliberately break it to shoehorn some stats in there

            var energyHistogram = ContinuousDataToHistogram(transformedEnergies);
            // var lifetimesHistogram = ContinuousDataToHistogram(lifetimes);

            DictionaryToCSV(energyHistogram, filename);
            // TODO: fix filenames so there's an energy one and a times one e.g histogram_energies_path28482.dat.csv                  
        }

        /// <summary>
        /// Takes a list of contiunous data, and returns it as a Histogram of frequencies
        /// sorted by bins; where each bin represents approximately 1% of the total dataset
        /// </summary>
        /// <param name="data">A list of contiuous data to take a histogram of</param>
        /// <returns>Returns a Histogram of frequencies of the inputted dataset</returns>
        static Dictionary<double, int> ContinuousDataToHistogram(List<double> data)
        {
            Dictionary<double, int> histogram = new Dictionary<double, int>();
            int numberOfBins = 100;
            // TODO: implement a clever algorithm to determine bin numbers and sizes
            data.Sort();

            double lowerBound = Math.Floor(data.Min());
            // use math floor so that the bins bounds are integers/decimals
            double upperBound = data.Max();
            double rangeOfData = upperBound - lowerBound;

            double binWidth = Math.Ceiling(rangeOfData / numberOfBins);

            // round the bin width to 0 decimal places if > 1 else round to 1 D.P (so the histograms are visually nicer)
            if (binWidth > 1)
            {
                binWidth = Math.Round(binWidth, 0);
            }
            else
            {
                binWidth = Math.Round(binWidth, 1);
            }

            // the interval between every 1% of the data

            for (int i = 0; i < numberOfBins; i++)
            {
                double binLowerBound = lowerBound + (i * binWidth);
                double binUpperBound = binLowerBound + binWidth;
                int frequency = data.Count(item => ((item >= binLowerBound) && (item < binUpperBound)) == true);
                histogram[binLowerBound] = frequency;

                // using the dictionary key of the lower bound of the bin, count how many items belong in the bin using a LINQ query
            }

            // data.Count(item => (item <= data.Min() + (interval * numberOfBins) == true);
            // ignore this for now

            // at this point histogram should have 100 keys, where they each represent the lower bound of an individual group
            // and a count associated with them

            return histogram;
        }

        /// <summary>
        /// Generic function to write a CSV file from a dictionary. Prompts user to open file after it has been written.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary's Key</typeparam>
        /// <typeparam name="TValue">The type of the dictionary's Value</typeparam>
        /// <param name="dictionary">The dictionary to be written</param>
        /// <param name="filename">The filename to write the file to</param>
        /// <remarks>Current limitations:
        /// - if your dictionary has over 1m key value pairs, it will hit excel's row limit; 
        /// - if any of the objects in Dictionary contain "," this will break the CSV
        /// - it only saves to the current working directory because i can't be fucked implementing
        /// a Windows forms filepicker and making the whole app windows forms</remarks>
        /// <returns>Returns -1 because i haven't actually written the bit to do return codes yet</returns>
        /// 
        static int DictionaryToCSV<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string filename)
        {

            String csv = String.Join(Environment.NewLine,
                                     dictionary.Select(d => d.Key + "," + d.Value + ",")
            );
            // this will break if any of the arguments have "," in them

            string fullFilename = "histogram_" + filename + "_.csv";

            while (true)
            {
                try
                {
                    System.IO.File.WriteAllText(fullFilename, csv);

                    // this next block about opening it should only execute if there's no exceptions thrown from writing the file

                    Console.WriteLine("Press 'y' to open the created file in your default editor.\nPress any other key to continue.");

                    // the false argument to Console.ReadKey supresses the user-inputted character from being displayed
                    if (Console.ReadKey(false).KeyChar == 'y')
                    {
                        Console.WriteLine("Loading file...");
                        System.Diagnostics.Process.Start(fullFilename);
                    }

                    break;
                    // break out from the loop if file successfully loads
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    if (ex.ToString().Contains("because it is being used by another process"))
                    {
                        MessageBox.Show("The file is open in another program, and cannot be written. Please close the file or close the console window to continue");
                        // i could add buttons and stuff for either "retry" or "abort" or something but
                        // 1) effort and 2) then i'd be making a full windows forms app anyway.
                    }
                    else
                    {
                        MessageBox.Show("An error occurred, please check the console for more. The file will be attempted to be written again when you click OK");
                    }
                    continue;
                }
            }
           
            // TODO: update this with proper return codes


            return -1;
        }

        /// <summary>
        /// Takes a file containing 2 times and an energy, and stores these values in a list
        /// </summary>
        /// <param name="filepath">Path to the file to be parsed</param>
        /// <returns>Returns all the Events as a List</returns>
        static List<Event> ParseFile(string filepath)
        {
            var events = new List<Event>();
            // List of events

            // declare our IEnumerable of fileLines to parallel iterate through outside try loop
            IEnumerable<string> fileLines = null;

            try
            {
                fileLines = File.ReadLines(filepath);
            }

            catch (System.IO.FileNotFoundException ex)
            {
                // Put the more specific exception first.
                 // TODO: (optional) implement exceptions fully. Prompt user for filepath (default to wherever i'm saving them), and if it doesn't exist 
                // return and reprompt
                System.Console.WriteLine(ex.ToString());
            }

            //     Normal regex ~= 5m40s to parse 1m lines
            //     Regex.Compiled ~= 3m30s
            //     Regex.Matches (read all 4 numbers separately) ~= didn't bother implementing lol string split blows this out the water
            //     String.Split ~= 3000ms (lmao)
            //     ParallelForeach String.Split still about ~3000ms so trivial benefit but i can't be arsed undoing that

            Parallel.ForEach(fileLines, line =>
            {
                // lock events to avoid race condition

                lock (events)
                {
                    if ((String.IsNullOrEmpty(line)) || (line[0] == '#'))
                    {
                        // do nothing
                    }
                    else
                    {
                        Event thisEvent = ParseLine(line);
                        events.Add(thisEvent);
                    }
                }
            });
            


                // ignore header lines (lines beginning with '#' or blank lines
                // for some reason it has the line as "" and it's not equal to null and doesn't terminate so that's why there's the length thing idfk


            return events;
            // should return a fully populated list
        }

        private static Event ParseLine(string line)
        {
            var thisEvent = new Event();
            // declare variable for the event we're reading off this line

            // use a regular expression to extract the 3 variables
            // regex for times needs to handle negative times too - theres some in the 1M data file which if
            // not accounted for give v > c (i.e. if the time were positive)


            var eventData = (from item in line.Split(' ')
                             where item != ""
                             select item).ToArray();
            /*
             * Should return a string array of the form:
             * eventData[0] = Event:
               eventData[1] = 24633
               eventData[2] = t1
               eventData[3] = =
               eventData[4] = -2.450189679
               eventData[5] = t2
               eventData[6] = = 
               eventData[7] = 7.559348615
               eventData[8] = E
               eventData[9] = = 
               eventData[10] = 2678.678075
             * 
             * the LINQ query to ignore "" means that two/three consecutive spaces shouldn't break it
             * 
             * this isn't quite as robust as the regex version but should be fast as fuck
             * 
             */

            try
            {
                thisEvent.time_1 = Double.Parse(eventData[4]);
                thisEvent.time_2 = Double.Parse(eventData[7]);
                thisEvent.energy = Double.Parse(eventData[10]);
            }
            catch (Exception)
            {
                Console.WriteLine("Malformed data on line with following contents:\n" + line);
                Console.WriteLine("Terminating, press any key to continue");
                Console.ReadKey();
                // read a key but dont' do anything with it; equiv to pause
                Application.Exit();
            }

            return thisEvent;
            
            // Old lol regex code for posterity
            //try
            //{

            //    thisEvent.time_1 = Convert.ToDouble(Regex.Match(line, @"(-?\d*.?\d*)(?=\st2\s=)", RegexOptions.Compiled).ToString());
            //    thisEvent.time_2 = Convert.ToDouble(Regex.Match(line, @"(-?\d*.?\d*)(?=\sE\s=)", RegexOptions.Compiled).ToString());
            //    thisEvent.energy = Convert.ToDouble(Regex.Match(line, @"(-?\d*.?\d*)$", RegexOptions.Compiled).ToString());

            //     TODO: (optional since the files aren't supposed to have errors) modify the regex to handle malformed files

            //     TODO: (optional) figure out how long the regex is taking and maybe use RegexOptions.Compiled isntead: http://www.dotnetperls.com/regexoptions-compiled


            //     try regex to assembly http://msdn.microsoft.com/en-us/library/9ek5zak6.aspx
            //     ^ dont do this its horrible and messy lol
            //}
            //catch (Exception)
            //{
            //     throw exception if values don't cast to floats
            //     write the errant values and line number here 
            //     Error on line xxx:
            //     t1 = xxx t2 = e = 
            //     <line>
            //    throw;
            //}
        }
    }
}
