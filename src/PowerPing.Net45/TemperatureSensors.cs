/*
 * 
 * License - PowerPing/TemperatureSensors
 * 
 * Copyright (c) 2018 https://github.com/reclaimed
 * 
 * Based on: Lasse Rasch https://stackoverflow.com/a/3114251/4389750
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.Management;

namespace PowerPing
{
    internal class TemperatureSensors
    {
        public double ReadCelsius { get; private set; }
        public double ReadFahrenheit { get; private set; }
        public double ReadKelvin { get; private set; }


        //public double CurrentValue { get; set; }
        public string InstanceName { get; set; }
        public static List<TemperatureSensors> Temperatures
        {
            get
            {


                List<TemperatureSensors> result = new List<TemperatureSensors>();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                foreach (ManagementObject obj in searcher.Get())
                {
                    double temp = Convert.ToDouble(obj["CurrentTemperature"].ToString());

                    //temp = (temp - 2732) / 10.0;
                    //result.Add(new TemperatureSensors { CurrentValue = temp, InstanceName = obj["InstanceName"].ToString() });

                    double kelvin = temp / 10;
                    double celsius = (temp / 10) - 273.15;
                    double fahrenheit = ((temp / 10) - 273.15) * 9 / 5 + 32;

                    result.Add(
                        new TemperatureSensors
                        {
                            ReadKelvin = kelvin,
                            ReadCelsius = celsius,
                            ReadFahrenheit = fahrenheit,
                            InstanceName = obj["InstanceName"].ToString()
                        }
                     );
                }
                return result;

            }
        }









    }
}