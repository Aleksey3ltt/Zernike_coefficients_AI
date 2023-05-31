import os;
import numpy as np
import matplotlib.pyplot as plt
from LightPipes import *
import math
import random
import json
from PIL import Image

wavelength=632.8*nm
size=5*mm
N=224
A=wavelength/(2*math.pi)

x_sep=0.5*mm
y_sep=0.5*mm
size_field=1.0*x_sep
sy=0*mm
f=5*cm
Dlens=0.45*mm
Nfields=8
size=(Nfields+1)*x_sep

F=Begin(size,wavelength,N)
Nfield=int(size_field/size*N)
Ffield=Begin(size_field,wavelength,Nfield)
Ffield=CircAperture(Ffield,Dlens/2)
Ffield=Lens(Ffield,f)
Phi=Phase(F)
F1=FieldArray2D(F,Ffield,Nfields,Nfields,x_sep,y_sep)
F1=CircAperture(F1,size/2,0,0)
Phi1=Phase(F1)
Ifields=Intensity(F1)
F1=Forvard(F1,f)
Iscreen1=Intensity(F1,1)

F2=Begin(size,wavelength,N)
F2=CircAperture(F2,size/2,0,0)

directory = os.getcwd()
#directory = os.path.dirname(directory)
print(directory)
c00= np.genfromtxt(directory+"\zernike.txt", delimiter='\n', dtype=float)
print("Read zernike.txt")

a0=[0] * 18
aN=[0] * 18
for Noll in range (4,22):
    # print(Noll)
    c0=c00[Noll-1]
    #c000=c00[Noll]
    print(c0)
    (nz,mz)=noll_to_zern(Noll)
    F2=Zernike(F2,nz,mz,size/2,c0,norm='False', units='lam')
    a0[Noll-4]=c0
    aN[Noll-4]=Noll

F3=CircAperture(F2,size/2,0,0)
Phi2=Phase(F3)
F3=MultIntensity(F3, Ifields)
F3=MultPhase(F3, Phi1)
F3=CircAperture(F3,size/2,0,0)
WFront=ZernikeFit(F3, aN, size/2, norm=True, units='lam')
F3=Fresnel(F3,f)
Iscreen2=Intensity(F3,1)

plt.imsave(directory+'\hart.png', Iscreen2, cmap='Greys')
plt.imsave(directory+'\Phi2.png', Phi2, cmap='jet')
np.save(directory+'\hart', Iscreen2)
np.save(directory+'\Phi2', Phi2)