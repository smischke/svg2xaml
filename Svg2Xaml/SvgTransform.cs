////////////////////////////////////////////////////////////////////////////////
//
//  SvgTransform.cs - This file is part of Svg2Xaml.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Media;

namespace Svg2Xaml
{

  //****************************************************************************
  abstract class SvgTransform
  {

    //==========================================================================
    public static SvgTransform Parse(string value)
    {
      if(value == null)
        throw new ArgumentNullException("value");
      
      value = value.Trim();
      if(value == "")
        throw new ArgumentException("value must not be empty", "value");

      List<SvgTransform> transforms = new List<SvgTransform>();

      string transform = value;
      while(transform.Length > 0)
      {
                
                int transformIndex = transform.IndexOf("(");
                string function = transform.Substring(0, transformIndex).Trim();
                transform = transform.Substring(transformIndex);

                int paramIndex = transform.IndexOf(")");
                string param = transform.Substring(1, paramIndex - 1);

                transform = transform.Substring(paramIndex + 1).TrimStart();
                transform = transform.TrimStart(',');

                switch (function)
                {
                    case "translate":
                        transforms.Add(SvgTranslateTransform.Parse(param));
                        break;
                    case "matrix":
                        transforms.Add(SvgMatrixTransform.Parse(param));
                        break;
                    case "scale":
                        transforms.Add(SvgScaleTransform.Parse(param));
                        break;
                    case "skewX":
                    case "skewY":
                        transforms.Add(SvgSkewTransform.Parse(param));
                        break;
                    case "rotate":
                        transforms.Add(SvgRotateTransform.Parse(param));
                        break;
                    default:
                        throw new ArgumentException(String.Format("Unsupported transform value: {0}", value));
                }
      }

      if(transforms.Count == 1)
        return transforms[0];
      else if(transforms.Count > 1)
        return new SvgTransformGroup(transforms.ToArray());

      throw new ArgumentException(String.Format("Unsupported transform value: {0}", value));
    }

    //==========================================================================
    public abstract Transform ToTransform();

  } // class Transform

}
