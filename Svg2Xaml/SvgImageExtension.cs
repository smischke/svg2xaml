////////////////////////////////////////////////////////////////////////////////
//
//  SvgImageExtension.cs - This file is part of Svg2Xaml.
//
//    Copyright (C) 2009,2011 Boris Richter <himself@boris-richter.net>
//
//  --------------------------------------------------------------------------
//
//  Svg2Xaml is free software: you can redistribute it and/or modify it under 
//  the terms of the GNU Lesser General Public License as published by the 
//  Free Software Foundation, either version 3 of the License, or (at your 
//  option) any later version.
//
//  Svg2Xaml is distributed in the hope that it will be useful, but WITHOUT 
//  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
//  FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public 
//  License for more details.
//  
//  You should have received a copy of the GNU Lesser General Public License 
//  along with Svg2Xaml. If not, see <http://www.gnu.org/licenses/>.
//
//  --------------------------------------------------------------------------
//
//  $LastChangedRevision$
//  $LastChangedDate$
//  $LastChangedBy$
//
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Windows.Markup;
using System.Resources;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Windows.Media;
using System.IO.Compression;
using System.Reflection;

namespace Svg2Xaml
{

    //****************************************************************************
    /// <summary>
    ///   A <see cref="MarkupExtension"/> for loading SVG images.
    /// </summary>
    [MarkupExtensionReturnType(typeof(DrawingImage))]
    public class SvgImageExtension
      : MarkupExtension
    {
        //==========================================================================
        private string m_Source = null;
        private bool m_IgnoreEffects = false;

        //==========================================================================
        /// <summary>
        ///   Initializes a new <see cref="SvgImageExtension"/> instance.
        /// </summary>
        public SvgImageExtension()
        {
            // ...
        }

        //==========================================================================
        /// <summary>
        ///   Initializes a new <see cref="SvgImageExtension"/> instance.
        /// </summary>
        /// <param name="uri">
        ///   The location of the SVG document.
        /// </param>
        public SvgImageExtension(string source)
        {
            m_Source = source;
        }

        //==========================================================================
        /// <summary>
        ///   Overrides <see cref="MarkupExtension.ProvideValue"/> and returns the 
        ///   <see cref="DrawingImage"/> the SVG document is rendered into.
        /// </summary>
        /// <param name="serviceProvider">
        ///   Object that can provide services for the markup extension; 
        ///   <paramref name="serviceProvider"/> is not used.
        /// </param>
        /// <returns>
        ///   The <see cref="DrawingImage"/> the SVG image is rendered into or 
        ///   <c>null</c> in case there has been an error while parsing or 
        ///   rendering.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var uri = GetUri(serviceProvider);

            string fileExt = Path.GetExtension(m_Source.ToString());
            bool isCompressed = !String.IsNullOrEmpty(fileExt) &&
                String.Equals(fileExt, ".svgz",
                StringComparison.OrdinalIgnoreCase);
            if (isCompressed)
            {
                using (Stream stream = Application.GetResourceStream(uri).Stream)
                    return SvgReader.Load(new GZipStream(stream, CompressionMode.Decompress), new SvgReaderOptions { IgnoreEffects = m_IgnoreEffects });
            }

            using (Stream stream = Application.GetResourceStream(uri).Stream)
                return SvgReader.Load(stream, new SvgReaderOptions { IgnoreEffects = m_IgnoreEffects });
        }

        //==========================================================================
        /// <summary>
        ///   Gets or sets the location of the SVG image.
        /// </summary>
        public string Source
        {
            get
            {
                return m_Source;
            }
            set
            {
                m_Source = value;
            }
        }

        /// <summary>
        /// Converts the SVG source file to <see cref="Uri"/>
        /// </summary>
        /// <param name="serviceProvider">
        /// Object that can provide services for the markup extension.
        /// </param>
        /// <returns>
        /// Returns the valid <see cref="Uri"/> of the SVG source path if
        /// successful; otherwise, it returns <see langword="null"/>.
        /// </returns>
        private Uri GetUri(IServiceProvider serviceProvider)
        {
            if (String.IsNullOrEmpty(m_Source))
            {
                return null;
            }

            Uri svgSource;
            if (Uri.TryCreate(m_Source, UriKind.RelativeOrAbsolute, out svgSource))
            {
                if (svgSource.IsAbsoluteUri)
                {
                    return svgSource;
                }
                else
                {
                    // Try getting a local file in the same directory....
                    string svgPath = m_Source;
                    if (m_Source[0] == '\\' || m_Source[0] == '/')
                    {
                        svgPath = m_Source.Substring(1);
                    }
                    svgPath = svgPath.Replace('/', '\\');

                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string localFile = Path.Combine(Path.GetDirectoryName(
                        assembly.Location), svgPath);

                    if (File.Exists(localFile))
                    {
                        return new Uri(localFile);
                    }

                    // Try getting it as resource file...
                    IUriContext uriContext = serviceProvider.GetService(
                        typeof(IUriContext)) as IUriContext;
                    if (uriContext != null && uriContext.BaseUri != null)
                    {
                        return new Uri(uriContext.BaseUri, svgSource);
                    }
                    else
                    {
                        string asmName = assembly.GetName().Name;
                        string uriString = String.Format(
                            "pack://application:,,,/{0};component/{1}",
                            asmName, m_Source);

                        return new Uri(uriString);
                    }
                }
            }

            return null;
        }

        //==========================================================================
        /// <summary>
        ///   Gets or sets whether SVG filter effects should be transformed into
        ///   WPF bitmap effects.
        /// </summary>
        public bool IgnoreEffects
        {
            get
            {
                return m_IgnoreEffects;
            }

            set
            {
                m_IgnoreEffects = value;
            }
        }
    } // class SvgImageExtension
}
 