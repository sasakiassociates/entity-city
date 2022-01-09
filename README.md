# **Entity City**

A protoype that explores augmenting data onto physical images by rendering LIDAR tiles with Unity ECS/DOTS and visualized using Vuforia Image Tracking. 

<br />

<p align="center">
  <img src="https://user-images.githubusercontent.com/30870056/148069342-a61de9ee-cdc2-42ad-9360-7ad5aff678bd.gif" alt="entityCity-colorramp"/>
</p>

<br />

### **about**
 Understanding the scale behind a 2D drawing is a common issue most people have when they look at a map. Typically a map gives a viewer a _top-down_ perspective to a space, which shows all the 3D stuff of a space to a limited 2D display. This probably helps with reading through these drawings quickly or with seeing a space holistically, but it's likely also the cause for the struggle with seeing things three-dimensionally. 
  <br />

  The approach to this project was to help people through that visual struggle by **augmenting spatial heightmap data** onto it's related **city map**. To keep the simplicity of a map, the heightmap data that was sourced through LIDAR tiles was visualized to look similarly to a _pin toy_.  
   
<br />

<p align="center">
  <img src="https://raw.githubusercontent.com/haitheredavid/content/main/EntityCity/PinToySearch.png" alt="PinToySearch"/>
</p>

<br />

### **details**  
- AR functioanlity is built out with Vuforia image tracking
- Map tiles Image Tracking library was created in MapBox 
- LIDAR tile set data sourced by (?)
- Developed in Unity with ECS / DOTS


### **Collaboration**  
This prototype was apart of larger social project by MakeTank called [City Print](cityprint), which was focused rebuilding the standard .3dm file of Boston by [crowd sourced support](tilecheckout). 

<p align="center">
  <img src="https://user-images.githubusercontent.com/30870056/148076647-749dd2ed-2101-4407-b77e-972fb93c6993.gif" alt="entityCity-maptiles"/>
</p>


[cityprint]: http://maketank-bsa.com/city-print
[tilecheckout]: http://maps.sasaki.com/tools/maketank/ 
[fluffysketch]: http://misc-tools.s3-website-us-east-1.amazonaws.com/fluffy_city/
