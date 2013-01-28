using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

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
            /// Kinetic energy of decayed particle. Units of MeV
            /// </summary>
            /// <remarks>
            /// This energy is a measured one.
            /// </remarks>
            public double energy;

            /// <summary>
            /// Get the velocity of the detected particle in this event as a proportion of the speed of light
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

            public double ParticleLiftetime()
            {
                // TODO: write this
                return -1;
            }

            /// <summary>
            /// Relativistic momentum of particle. Units of MeV/c
            /// </summary>
            /// <returns></returns>
            public double Momentum()
            {
                return this.Gamma() * positronRestMass * this.Beta();
            }

            public double TransformEnergy()
            {
                return this.Gamma() * (this.energy - (this.Momentum() * this.Beta()));
            }

        }

        
        static void Main(string[] args)
        {
            string filepath = @"C:\Users\Josh\Downloads\p3data7.dat";
            string filename = Path.GetFileName(filepath);

            List<Event> events = ParseFile(filepath);


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

            Console.WriteLine("Number of bad events: " + numberOfBadEvents);
            Console.WriteLine("Average energies" + averageEvergy);
            Console.WriteLine("Average t2: " + averageT2);

            Console.WriteLine("Transforming Event [0]");
            Console.WriteLine("Beta, Gamma, Momentum, Energy Transform");
            Console.WriteLine("{0}, {1}, {2}, {3}", events[0].Beta(), events[0].Gamma(), events[0].Momentum(), events[0].TransformEnergy());

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

            var energyHistogram = ContinuousDataToHistogram(transformedEnergies);
            var lifetimesHistogram = ContinuousDataToHistogram(lifetimes);

            DictionaryToCSV(energyHistogram, filename);
            // todo: fix filenames so there's an energy one and a times one e.g histogram_energies_path28482.dat.csv

                                      
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

            data.Sort();   
            
            double lowerBound = data.Min();
            double upperBound = data.Max();
            double rangeOfData = lowerBound - upperBound;
                        
            int interval = Convert.ToInt32(Math.Ceiling(rangeOfData / (data.Count * numberOfBins)));

            // the interval between every 1% of the data

            for (int i = 0; i < data.Count(); i += interval)
            {
                double binLowerBound = data.Min() + (i * interval);
                double binUpperBound = data.Min() + ((i + 1) * interval);
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

        static int DictionaryToCSV<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string filename)
        {
            // Generic function that writes a CSV file from a Dictionary
            // (by generic it takes literally any type of variable as an argument which is pretty damn awesome; strings, floats ints, custom types, you name it, it works)
            // C# is the best

            // Limitations: if your dictionary has over 1m key value pairs, it will hit excel's row limit
            // If any of the objects in Dictionary contain "," this will break the CSV
            String csv = String.Join(Environment.NewLine,
                                     dictionary.Select(d => d.Key + "," + d.Value + ",")
            );
            // this will break if any of the arguments have "," in them

            System.IO.File.WriteAllText("histogram_" + filename + "_.csv", csv);

            // TODO: add exceptions here to catch when a file is open and unable to be overwritten
            // so i don't end up with runtime errors everywhere when i'm debugging and forget to close excel laffo
            
            // TODO: update this with proper return codes
            // TODO: prompt the user to see if they want to open the file after writing
            
            return -1;
        }

        /// <summary>
        /// Takes a file containing 2 times and an energy, and stores these values in a list
        /// </summary>
        /// <param name="filepath">Path to the file to be parsed</param>
        /// <returns>Returns all the Events as a List</returns>
        static List<Event> ParseFile(string filepath)
        {
            System.IO.StreamReader inputFile = null;

            var events = new List<Event>();
            // List of events

            try
            {
                inputFile = new System.IO.StreamReader(filepath);
            }

            catch (System.IO.FileNotFoundException ex)
            {
                // Put the more specific exception first.
                 // TODO: implement exceptions fully. Prompt user for filepath (default to wherever i'm saving them), and if it doesn't exist 
                // return and reprompt
                System.Console.WriteLine(ex.ToString());
            }

            string line;
            
            // read the full file line by line
            while ((line = inputFile.ReadLine()) != null)
            {
                // ignore header lines (lines beginning with '#' or blank lines
                // for some reason it has the line as "" and it's not equal to null and doesn't terminate so that's why there's the length thing idfk
                if ((line.Length == 0) || (line[0] == '#'))
                {
                    continue;
                }
                
                var thisEvent = new Event();
                // declare variable for the event we're reading off this line
                
                // use a regular expression to extract the 3 variables
                try
                {
                    thisEvent.time_1 = Convert.ToDouble(Regex.Match(line, @"(\d*.?\d*)(?=\st2\s=)").ToString());
                    thisEvent.time_2 = Convert.ToDouble(Regex.Match(line, @"(\d*.?\d*)(?=\sE\s=)").ToString());
                    thisEvent.energy = Convert.ToDouble(Regex.Match(line, @"(-?\d*.?\d*)$").ToString());

                    // TODO: modify the regex to handle malformed files

                    // TODO: (optional) figure out how long the regex is taking and maybe use RegexOptions.Compiled isntead: http://www.dotnetperls.com/regexoptions-compiled
                }
                catch (Exception)
                {
                    // throw exception if values don't cast to floats
                    // write the errant values and line number here 
                    // Error on line xxx:
                    // t1 = xxx t2 = e = 
                    // <line>
                    throw;
                }

                events.Add(thisEvent);
                // append this event
            }

            return events;
            // should return a fully populated list
        }
    }
}
