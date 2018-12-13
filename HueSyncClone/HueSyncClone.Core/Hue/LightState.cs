using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace HueSyncClone.Hue
{
    /// <summary>
    /// Details the state of the light.
    /// </summary>
    [DebuggerDisplay("{DebugString,nq}")]
    public class LightState
    {
        /// <summary>
        /// Get or set On/Off state of the light.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On/Off state of the light. On=true, Off=false
        /// </para>
        /// </remarks>
        public bool On { get; set; }

        /// <summary>
        /// Get or set brightness of the light.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Brightness of the light.
        /// This is a scale from the minimum brightness the light is capable of, 1, to the maximum capable brightness, 254.
        /// </para>
        /// </remarks>
        [JsonProperty("bri")]
        public int Brightness { get; set; }

        /// <summary>
        /// Get or set hue of the light.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Hue of the light.
        /// This is a wrapping value between 0 and 65535.
        /// Note, that hue/sat values are hardware dependent which means that programming two devices with the same value does not guarantee that they will be the same color.
        /// Programming 0 and 65535 would mean that the light will resemble the color red, 21845 for green and 43690 for blue.
        /// </para>
        /// </remarks>
        public int Hue { get; set; }

        /// <summary>
        /// Get or set saturation of the light.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Saturation of the light. 254 is the most saturated (colored) and 0 is the least saturated (white).
        /// </para>
        /// </remarks>
        [JsonProperty("sat")]
        public int Saturation { get; set; }

        /// <summary>
        /// Get or set the dynamic effect of the light.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The dynamic effect of the light, can either be <see cref="LightEffect.None"/> or <see cref="LightEffect.ColorLoop"/>.
        /// If set to <see cref="LightEffect.ColorLoop"/>, the light will cycle through all hues using the current brightness and saturation settings.
        /// </para>
        /// </remarks>
        public LightEffect Effect { get; set; }

        /// <summary>
        /// Get or set the x and y coordinates of a color in CIE color space.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The x and y coordinates of a color in CIE color space.
        /// The first entry is the x coordinate and the second entry is the y coordinate.
        /// Both x and y are between 0 and 1.
        /// Using CIE xy, the colors can be the same on all lamps if the coordinates are within every lamps gamuts(example: “xy”:[0.409,0.5179] is the same color on all lamps).
        /// If not, the lamp will calculate it’s closest color and use that.The CIE xy color is absolute, independent from the hardware.
        /// </para>
        /// </remarks>
        public XyColor Xy { get; set; }

        /// <summary>
        /// Get or set the Mired Color temperature of the light.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Mired Color temperature of the light.
        /// 2012 connected lights are capable of 153 (6500K) to 500 (2000K).
        /// </para>
        /// </remarks>
        [JsonProperty("ct")]
        public ColorTemperature ColorTemperature { get; set; }

        /// <summary>
        /// Get or set the alert effect.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The alert effect, which is a temporary change to the bulb’s state.
        /// </para>
        /// <para>
        /// Note that this contains the last alert sent to the light and not its current state.i.e.After the breathe cycle has finished the bridge does not reset the alert to <see cref="LightAlertEffect.None"/>.
        /// </para>
        /// </remarks>
        public LightAlertEffect Alert { get; set; }

        /// <summary>
        /// Get or set the color mode in which the light is working, this is the last command type it received.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Indicates the color mode in which the light is working, this is the last command type it received.
        /// Values are “hs” for Hue and Saturation, “xy” for XY and “ct” for Color Temperature.
        /// This parameter is only present when the light supports at least one of the values.
        /// </para>
        /// </remarks>
        public ColorMode ColorMode { get; set; }

        /// <summary>
        /// Get or set a light can be reached by the bridge.
        /// </summary>
        public bool Reachable { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebugString
        {
            get
            {
                if (!On) return "Off";

                switch (ColorMode)
                {
                    case ColorMode.HueSaturation:
                        return $"hs({Hue}, {Math.Round(Saturation / 2.54, 2)}%, {Math.Round(Brightness / 2.54, 2)}%)";
                    case ColorMode.Xy:
                        return $"xy({Xy.X}, {Xy.Y}, {Math.Round(Brightness / 2.54, 2)}%)";
                    case ColorMode.ColorTemperature:
                        return $"ct({ColorTemperature.Mired}M, {Math.Round(Brightness / 2.54, 2)}%)";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}