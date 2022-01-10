# **Entity City**

A prototype that explores augmenting data onto physical images by rendering LIDAR tiles with Unity ECS/DOTS and visualized using Vuforia Image Tracking.

<br />

<p align="center">
  <img src="https://user-images.githubusercontent.com/30870056/148069342-a61de9ee-cdc2-42ad-9360-7ad5aff678bd.gif" alt="entityCity-colorramp"/>
</p>

<br />

### **about**
Understanding the scale behind a 2D drawing is a common issue most people have when they look at a map. Typically a map gives a viewer a _top-down_ perspective to a space, which shows all the 3D stuff of a space to a limited 2D display. This probably helps with reading through these drawings quickly or with seeing a space holistically, but it's likely also the cause for the struggle with seeing things three-dimensionally.

<br />

<p align="center">
  <img src="https://raw.githubusercontent.com/haitheredavid/content/main/EntityCity/EntityCity-Sample-Zakim.gif" />
</p>

<br />

The approach to this project was to help people through that visual struggle by **augmenting spatial heightmap data** onto it's related **city map**. Using simple shapes and textures can display the general form and spatial qualities of a city [similarly to this sketch by Ken Goulding](fluffysketch). To keep the simplicity of a map, the heightmap data that was sourced through LIDAR tiles was visualized to look similarly to a [_pin toy_](pintoysearch).

<br />

### **details**

- Currently this project is built with Vuforia and targets ARCore as it's main AR library
- The images used to anchor the LIDAR tiles was created and styled with MapBox
- Developed in Unity with ECS / DOTS
- LIDAR tile set [data sourced](https://www.fisheries.noaa.gov/inport/item/49846)  

<br />


<p align="center">
  <img src="https://raw.githubusercontent.com/haitheredavid/content/main/EntityCity/entityCity-maptiles.gif" width="40%" height="40%"/>
  <img src="https://raw.githubusercontent.com/haitheredavid/content/main/EntityCity/EntityCity-LidarTiles.gif" width="40%" height="40%"/>
</p>

<br />

### **team**
[Ken Goulding](https://github.com/gouldingken), [Eric Youngberg](https://github.com/ericyoungberg), [David Morgan](https://github.com/haitheredavid)   


### **collaboration**
This prototype was apart of larger social project by MakeTank called [City Print](cityprint), which was focused rebuilding the standard .3dm file of Boston by [crowd sourced support](tilecheckout).

<br />


[cityprint]: http://maketank-bsa.com/city-print
[tilecheckout]: http://maps.sasaki.com/tools/maketank/
[fluffysketch]: http://misc-tools.s3-website-us-east-1.amazonaws.com/fluffy_city/
[pintoysearch]: https://www.google.com/search?q=pin+toy&tbm=isch&ved=2ahUKEwivoPm5uqb1AhUMD60KHZS0BDYQ2-cCegQIABAA&oq=pin+toy&gs_lcp=CgNpbWcQAzIHCCMQ7wMQJzIFCAAQgAQyBQgAEIAEMgUIABCABDIFCAAQgAQyBQgAEIAEMgUIABCABDIGCAAQBxAeMgYIABAHEB4yBggAEAcQHlDFBljFBmCsCGgAcAB4AIABhAGIAfsBkgEDMC4ymAEAoAEBqgELZ3dzLXdpei1pbWfAAQE&sclient=img&ei=xcTbYe-pHYyetAWU6ZKwAw&bih=929&biw=854
