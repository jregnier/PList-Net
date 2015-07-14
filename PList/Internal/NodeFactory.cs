﻿using System;
using System.Collections.Generic;
using PListNet.Collections;
using PListNet.Exceptions;
using PListNet.Primitives;

namespace PListNet.Internal
{
	/// <summary>
	/// Singleton class which generates concrete <see cref="T:PListNet.PNode"/> from the Tag or TypeCode
	/// </summary>
	internal static class NodeFactory
	{
		private static readonly Dictionary<string, Type> _xmlTags = new Dictionary<string, Type>();
		private static readonly Dictionary<byte, Type> _binaryTags = new Dictionary<byte, Type>();

		/// <summary>
		/// Initializes the <see cref="NodeFactory"/> class.
		/// </summary>
		static NodeFactory()
		{
			Register(new PListDict());
			Register(new PListInteger());
			Register(new PListReal());
			Register(new PListString());
			Register(new PListArray());
			Register(new PListData());
			Register(new PListDate());

			Register("string", 5, new PListString());
			Register("ustring", 6, new PListString());

			Register("true", 0, new PListBool());
			Register("false", 0, new PListBool());
		}

		/// <summary>
		/// Registers the specified element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="node">The node.</param>
		private static void Register<T>(T node) where T : PNode, new()
		{
			if (!_xmlTags.ContainsKey(node.XmlTag)) _xmlTags.Add(node.XmlTag, node.GetType());
			if (!_binaryTags.ContainsKey(node.BinaryTag)) _binaryTags.Add(node.BinaryTag, node.GetType());
		}

		/// <summary>
		/// Registers the specified element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="xmlTag">The tag.</param>
		/// <param name="binaryTag">The type code.</param>
		/// <param name="node">The element.</param>
		private static void Register<T>(string xmlTag, byte binaryTag, T node) where T : PNode, new()
		{
			if (!_xmlTags.ContainsKey(xmlTag)) _xmlTags.Add(xmlTag, node.GetType());
			if (!_binaryTags.ContainsKey(binaryTag)) _binaryTags.Add(binaryTag, node.GetType());
		}

		/// <summary>
		/// Creates a concrete <see cref="T:PListNet.PNode"/> object secified specified by it's typecode.
		/// </summary>
		/// <param name="binaryTag">The typecode of the element.</param>
		/// <param name="length">The length of the element 
		/// (required only for <see cref="T:PListNet.Primitives.PListBool"/>, <see cref="T:PListNet.Primitives.PListNull"/>
		/// and <see cref="T:PListNet.Primitives.PListFill"/>).</param>
		/// <returns>The created <see cref="T:PListNet.PNode"/> object</returns>
		public static PNode Create(byte binaryTag, int length)
		{
			if (binaryTag == 0 && length == 0x00) return new PListNull();
			if (binaryTag == 0 && length == 0x0F) return new PListFill();

			if (binaryTag == 6) return new PListString { IsUtf16 = true };

			if (_binaryTags.ContainsKey(binaryTag))
			{
				return (PNode) Activator.CreateInstance(_binaryTags[binaryTag]);
			}

			throw new PListFormatException(string.Format("Unknown node - binary tag {0}", binaryTag));
		}

		/// <summary>        
		/// Creates a concrete <see cref="T:PListNet.PNode"/> object secified specified by it's tag.
		/// </summary>
		/// <param name="tag">The tag of the element.</param>
		/// <returns>The created <see cref="T:PListNet.PNode"/> object</returns>
		public static PNode Create(string tag)
		{
			if (_xmlTags.ContainsKey(tag))
			{
				return (PNode) Activator.CreateInstance(_xmlTags[tag]);
			}

			throw new PListFormatException(string.Format("Unknown node - XML tag \"{0}\"", tag));
		}

		/// <summary>
		/// Creates a <see cref="T:PListNet.PNode"/> object used for exteded length information.
		/// </summary>
		/// <param name="length">The exteded length information.</param>
		/// <returns>The <see cref="T:PListNet.PNode"/> object used for exteded length information.</returns>
		public static PNode CreateLengthElement(int length)
		{
			return new PListInteger(length);
		}

		/// <summary>
		/// Creates a <see cref="T:PListNet.PNode"/> object used for dictionary keys.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The <see cref="T:PListNet.PNode"/> object used for dictionary keys.</returns>
		public static PNode CreateKeyElement(string key)
		{
			return new PListString(key);
		}
	}
}