using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkySigService
{
    public interface ISDR
    {
        /// <summary>
        /// The type of tuner that we're connected to
        /// </summary>
        TunerType TunerType { get; }

        /// <summary>
        /// The number of gain levels this tuner has
        /// </summary>
        uint TunerGainLevels { get; }

        /// <summary>
        /// Center Frequency the SDR is Tuned To in Hz
        /// From: https://osmocom.org/projects/rtl-sdr/repository/revisions/master/entry/include/rtl-sdr.h#L255
        /// /*!
        /// * Set the sample rate for the device, also selects the baseband filters
        /// * according to the requested sample rate for tuners where this is possible.
        /// *
        /// * \param dev the device handle given by rtlsdr_open()
        /// * \param samp_rate the sample rate to be set, possible values are:
        /// *                     225001 - 300000 Hz
        /// *                     900001 - 3200000 Hz
        /// * sample loss is to be expected for rates > 2400000
        /// * \return 0 on success, -EINVAL on invalid rate
        /// */
        /// </summary>
        uint CenterFrequency { get; set; }

        /// <summary>
        /// Sample rate of the SDR in Samples per Second
        /// </summary>
        uint SampleRate { get; set; }

        /// <summary>
        /// Whether or not Auto Gain Control is enabled
        /// </summary>
        bool AutoGainControl { get; set; }

        /// <summary>
        /// The current Tuner Gain Index
        /// </summary>
        uint TunerGain { get; set; }
       
        Stream IQ { get; }
    }
}
