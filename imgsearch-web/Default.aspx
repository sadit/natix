<%@ Page Language="C#" Inherits="imgsearchweb.Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head runat="server">
	<title>Image Search by Content with NATIX</title>
	<style type="text/css">
	body {
		font-family: helvetica;
		font-size: small;
		background-color: white;
	}
	.results {
		text-align: center;
		margin-left: auto;
		margin-right: auto;
		display: block;
		padding: 0.2cm;
	}
	.thinborder {
		border: 1pt solid rgb(230,230,250);
		padding: 0.2cm;		
	}
	.title {
		border: 1pt solid black;
		padding: 0.2cm;
		text-align: center;
	}
	.annotation {
		font-size: small;
		width: 60%;
		margin-left: auto;
		margin-right: auto;
		padding: 0.2cm;
	}	
	</style>
</head>
<body>

<div class="title">
	<div style="font-weight: bold;">Image Search by Content using NATIX</div>
	<div style="text-align: right; width: 90%;">
		<a href="?Random=true">Random</a> |
		<a href="?About=true">About</a> |
		<a href="mailto:donsadit@gmail.com">Contact</a> |
		<a href="../../">Natix home</a>
	</div>
</div>
<br/>

<div id="TextData" runat="server"></div>			
	<div class="results thinborder">
		<span id="Welcome" style="text-align: left;" runat="server">Image Search by Content using NATIX</span><br/>
		<span id="Warning">This site contains third party images, and they can contain adult content. Please do not use it if you do not reach the full age. </span>
		<div id="Result" runat="server" style="margin-left: auto; margin-right: auto; with=80%;">
		</div>
		
		<div style="font-size: smaller; width: 60%; margin-left: auto; margin-right: auto;" class="thinborder">
		<p>		
		These images are miniature versions of the original ones, and cames from the site
		<a href="http://www.flickr.com">www.flickr.com</a>.
		All rights are reserved to the author of the original image, which is directly
		available on the Flickr site through a hypertext link.
		</p>

		<p>
			The <a href="http://www.natix.org">natix.org</a> project.
		</p>
		</div>
</div>

</body>
