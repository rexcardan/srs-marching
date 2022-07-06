## AAPM 2022 Abstract Submission
The following is the source code for the submitted abstract at AAPM's Annual Meeting 2022 in Washington DC. 
Simply clone, build and run to get the results demonstrated in the abstract.

#### Standardized Open-Source Volume Calculations for Small Voxelized Volumes
##### Rex Cardan, Richard Popple
##### Purpose: 
The discrepancy of small volume calculations has been shown to be large between various systems used in radiation oncology. These discrepancies obfuscate outcome analysis and intersystem comparisons of plan quality. A technique is needed to provide a standard for volume metrics.
##### Methods: 
A set of perfect spherical dose distributions of varying diameters were projected into a 3D voxelized grid. Using a marching cubes algorithm, a triangular mesh was created at the 75% isodose level. The mesh volume was calculated and compared to the known “golden” volume of the sphere. The relationship was plotted to determine the conversion between the calculated and true volumes.
##### Results:  
The raw volumes were found to differ by 37.8% for a 1.5 mm diameter sphere and 0.5% for the largest 50 mm sphere, with an average error of 9.72 ± 11.04%. The conversion factor from mesh volume to true volume was found to follow the power law (R2 = 0.989) by the equation V=m*(1+ 0.9574*m^(-0.4103)) where V is the true volume and m is the mesh volume. The discrepancy after applying conversion was an average difference of 0.20 ± 1.34%, with a maximum of 3.11%.
##### Conclusion: 
Using a simple correction factor, triangular meshes produced by marching cubes can produce highly accurate volume metrics for small voxelized volumes. An open-source algorithm for this technique is provided at http://github.com/rexcardan/srs-marching.git.

![poster image](https://github.com/rexcardan/srs-marching/blob/master/Resources/smallVolumes_Cardan_Popple_AAPM2022.png?raw=true)
