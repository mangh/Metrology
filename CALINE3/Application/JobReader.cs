/*******************************************************************************

    Units of Measurement for C# applications applied to
    the CALINE3 Model algorithm.

    For more information on CALINE3 and its status see:
    • https://www.epa.gov/scram/air-quality-dispersion-modeling-alternative-models#caline3
    • https://www.epa.gov/scram/2017-appendix-w-final-rule.

    Copyright (C) mangh

    This program is provided to you under the terms of the license 
    as published at https://github.com/mangh/metrology.

********************************************************************************/

using System.IO;

namespace CALINE3
{
    public class JobReader
    {
        #region Fields
        private readonly TextReader _input;
        private int ordinal = 0;    // JOB ordinal number
        #endregion

        #region Constructor(s)
        public JobReader(TextReader input)
        {
            _input = input;
        }
        #endregion

        #region Method(s)
        /// <summary>Read <see cref="Job"/> data (including 
        /// <see cref="Receptor"/> set, RUN parameters, <see cref="Link"/> collection and <see cref="Meteo"/> conditions).
        /// </summary>
        /// <returns><see cref="Job"/> instance (<c>null</c> on EOF).</returns>
        /// <example>Sample input JOB line:
        /// <code>EXAMPLE ONE                              60. 10.   0.   0. 1        1.</code>
        /// </example>
        public Job? Read()
        {
            string? line = _input.ReadLine();
            if (line is not null)
            {
                Job job = new(
                    /*ORDINAL*/ ordinal++,
                    /*JOB*/  line.Substring(0, 40).Trim(),
                    /*ATIM*/ (Minute)double.Parse(line.Substring(40, 4)),
                    /*Z0*/   (Centimeter)double.Parse(line.Substring(44, 4)),
                    /*VS*/   (Centimeter_Sec)double.Parse(line.Substring(48, 5)),
                    /*VD*/   (Centimeter_Sec)double.Parse(line.Substring(53, 5)),
                    /*NR*/   int.Parse(line.Substring(58, 2)),
                    /*SCAL*/ double.Parse(line.Substring(60, 10))
                );

                if (ReadReceptors(job) && 
                    ReadRunParameters(job, out int NL, out int NM) && 
                    ReadLinks(job, NL) && 
                    ReadMeteos(job, NM))
                {
                    return job;
                }
            }
            return null;
        }

        /// <summary>
        /// Read all <see cref="Receptor"/> lines (in the number <see cref="Job.NR"/> as declared in the parent <see cref="Job"/>).
        /// </summary>
        /// <param name="job">parent <see cref="Job"/>.</param>
        /// <returns><c>true</c> when all declared <see cref="Receptor"/> lines have been read; <c>false</c> otherwise.</returns>
        /// <example>Sample input RECEPTOR line:
        /// <code>RECP. 1                    30.        0.       1.8</code>
        /// </example>
        private bool ReadReceptors(Job job)
        {
            for (int ordinal = 0; ordinal < job.NR; ordinal++)
            {
                string? line = _input.ReadLine();
                if (line is null) return false;
                job.Receptors.Add(
                    new Receptor(
                        /*ORDINAL*/ ordinal,
                        /*RCP*/ line.Substring(0, 20).Trim(),
                        /*XR*/ (Meter)(job.SCAL * double.Parse(line.Substring(20, 10))),
                        /*YR*/ (Meter)(job.SCAL * double.Parse(line.Substring(30, 10))),
                        /*ZR*/ (Meter)(job.SCAL * double.Parse(line.Substring(40, 10)))
                    )
                );
            }
            return true;
        }

        /// <summary>
        /// Read RUN parameters: TITLE, NL (number of links) and NM (number of meteo conditions).
        /// </summary>
        /// <param name="job">parent <see cref="Job"/>.</param>
        /// <returns><c>true</c> when parameters have been read; <c>false</c> otherwise.</returns>
        /// <example>Sample input RUN line:
        /// <code>URBAN LOCATION: INTERSECTION              6  1</code>
        /// </example>
        private bool ReadRunParameters(Job job, out int NL, out int NM)
        {
            string? line = _input.ReadLine();
            if (line is null)
            {
                NL = 0;
                NM = 0;
                return false;
            }
            else
            {
                job.RUN = line.Substring(0, 40).Trim();
                NL = int.Parse(line.Substring(40, 3));
                NM = int.Parse(line.Substring(43, 3));
                return true;
            }
        }

        /// <summary>
        /// Read all <see cref="Link"/> lines (in the number of <paramref name="NL"/> as previously declared).
        /// </summary>
        /// <param name="job">parent <see cref="Job"/>.</param>
        /// <param name="NL">number of <see cref="Link"/> lines to be read.</param>
        /// <returns><c>true</c> when all declared <see cref="Link"/> lines have been read; <c>false</c> otherwise.</returns>
        /// <example>Sample input LINK line:
        /// <code>LINK A              AG     0. -5000.     0.  5000.   7500. 30.  0. 30.</code>
        /// </example>
        private bool ReadLinks(Job job, int NL)
        {
            if (job.RUN is null) return false;

            for (int ordinal = 0; ordinal < NL; ordinal++)
            {
                string? line = _input.ReadLine();
                if (line is null) return false;
                job.Links.Add(
                    new Link(
                        /*ORDINAL*/ ordinal,
                        /*LNK*/  line.Substring(0, 20).Trim(),
                        /*TYP*/  line.Substring(20, 2),
                        /*XL1*/  (Meter)(job.SCAL * double.Parse(line.Substring(22, 7))),
                        /*YL1*/  (Meter)(job.SCAL * double.Parse(line.Substring(29, 7))),
                        /*XL2*/  (Meter)(job.SCAL * double.Parse(line.Substring(36, 7))),
                        /*YL2*/  (Meter)(job.SCAL * double.Parse(line.Substring(43, 7))),
                        /*VPHL*/ (Event_Hour)double.Parse(line.Substring(50, 8)),
                        /*EFL*/  (Gram_Mile)double.Parse(line.Substring(58, 4)),
                        /*HL*/   (Meter)(job.SCAL * double.Parse(line.Substring(62, 4))),
                        /*WL*/   (Meter)(job.SCAL * double.Parse(line.Substring(66, 4)))
                    )
                );
            }
            return true;
        }

        /// <summary>
        /// Read all <see cref="Meteo"/> lines (in the number of <paramref name="NM"/> as previously declared).
        /// </summary>
        /// <param name="job">parent <see cref="Job"/>.</param>
        /// <param name="NM">number of <see cref="Meteo"/> conditions to be read.</param>
        /// <returns><c>true</c> when all declared <see cref="Meteo"/> lines have been read; <c>false</c> otherwise.</returns>
        /// <example>Sample input METEO line: 
        /// <code>  1.270.6 1000. 3.0</code>
        /// </example>
        private bool ReadMeteos(Job job, int NM)
        {
            if (job.RUN is null) return false;

            for (int ordinal = 0; ordinal < NM; ordinal++)
            {
                string? line = _input.ReadLine();
                if (line is null) return false;
                job.Meteos.Add(
                    new Meteo(
                        /*ORDINAL*/ ordinal,
                        /*U   */ (Meter_Sec)double.Parse(line.Substring(0, 3)),
                        /*BRG */ (Degree)double.Parse(line.Substring(3, 4)),
                        /*CLAS*/ int.Parse(line.Substring(7, 1)),
                        /*MIXH*/ (Meter)double.Parse(line.Substring(8, 6)),
                        /*AMB */ (PPM)double.Parse(line.Substring(14, 4))
                    )
                );
            }
            return true;
        }
        #endregion
    }
}
