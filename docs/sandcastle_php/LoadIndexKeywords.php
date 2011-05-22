<?php

//=============================================================================
// System  : Sandcastle Help File Builder (PHP port)
// Author  : Latif Khalifa <latifer@streamgrid.net>, All rights reserved
// Copyright (c) 2011, Latif Khalifa <latifer@streamgrid.net>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Based on C# version by Eric Woodruff  (Eric@EWoodruff.us) from code by Ferdinand Prantl
// Copyright 2008, Eric Woodruff, All rights reserved

$ret = "";

$ki = DOMDocument::load("WebKI.xml");
$xpath = new DOMXpath($ki);
$root = $xpath->query("*" );

$startIndex = 0;

if (isset($_GET["StartIndex"])) {
  $startIndex = (int)$_GET["StartIndex"];
}

$endIndex = $startIndex + 128;

if ($endIndex > $root->length)
  $endIndex = $root->length;

if ($startIndex > 0) {
  $prev = $startIndex - 128;
  if ($prev < 0) $prev = 0;
  $ret .= "<div class=\"IndexItem\">\r\n" .
    "<span>&nbsp;</span><a class=\"UnselectedNode\" " .
    "onclick=\"javascript: return PopulateIndex($prev);\" " .
    "href=\"#\"><b><< Previous page</b></a>\r\n</div>\r\n";
}


$title = "";

while($startIndex < $endIndex) {
  $node = $root->item($startIndex);
  $url = $node->getAttribute("Url");

  if (!$url) {
    $url = "#";
    $target = "";
  } else {
    $target = " target=\"TopicContent\"";
  }

  $ret .= sprintf("<div class=\"IndexItem\">\r\n" .
		  "<span>&nbsp;</span><a class=\"UnselectedNode\" " .
		  "onclick=\"javascript: return SelectIndexNode(this);\" " .
		  "href=\"%s\"%s>%s</a>\r\n", $url, $target,
		  htmlentities($node->getAttribute("Title")));

  if ($node->hasChildNodes()) {
    foreach($node->childNodes as $subNode) {
      $ret .= sprintf("<div class=\"IndexSubItem\">\r\n" .
		      "<img src=\"Item.gif\"/><a class=\"UnselectedNode\" " .
		      "onclick=\"javascript: return SelectIndexNode(this);\" " .
		      "href=\"%s\" target=\"TopicContent\">%s</a>\r\n</div>\r\n",
		      $subNode->getAttribute("Url"),
		      htmlentities($subNode->getAttribute("Title")));
    }
  }

  $ret .= "</div>\r\n";
  
  $startIndex++;
}

if ($startIndex < $root->length) {
  $ret .= "<div class=\"IndexItem\">\r\n" .
    "<span>&nbsp;</span><a class=\"UnselectedNode\" " .
    "onclick=\"javascript: return PopulateIndex($startIndex);\" " .
    "href=\"#\"><b>Next page >></b></a>\r\n</div>\r\n";
}

print $ret;
