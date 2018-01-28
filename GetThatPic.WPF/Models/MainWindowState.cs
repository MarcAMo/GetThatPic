﻿// <copyright file="MainWindowState.cs" company="Marc A. Modrow">
// Copyright (c) 2018 All Rights Reserved
// <author>Marc A. Modrow</author>
// </copyright>
using System;
using System.Windows.Media.Imaging;
using GetThatPic.Data.Structure;
using GetThatPic.Parsing;
using GetThatPic.Parsing.Models;

namespace GetThatPic.WPF.Models
{
    /// <summary>
    /// Main Window State Model - Keeps track of what is happening.
    /// </summary>
    public class MainWindowState
    {
        /// <summary>
        /// Gets history buffer.
        /// </summary>
        /// <value>
        /// The history.
        /// </value>
        public RingBuffer<ImageEntry> History { get; } = new RingBuffer<ImageEntry>(50);

        /// <summary>
        /// Gets the link parser.
        /// </summary>
        /// <value>
        /// The link parser.
        /// </value>
        public Link LinkParser { get; } = new Link();

        /// <summary>
        /// Gets or sets the preview item.
        /// </summary>
        /// <value>
        /// The preview item.
        /// </value>
        public ImageEntry PreviewItem { get; set; }
    }
}
