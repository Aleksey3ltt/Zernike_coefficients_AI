from tensorflow.keras import layers
from tensorflow.keras import Model
import cv2
import os
import numpy as np
import tensorflow as tf
import matplotlib.pyplot as plt
from tensorflow.keras.applications import EfficientNetB0
import time
import imageio

from LightPipes import *
import math
import random
import json
from PIL import Image as im

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

directory = os.getcwd()
print(directory)
c00= np.genfromtxt(directory+"\zernike.txt", delimiter='\n', dtype=float)
print("Read zernike.txt")

a0=[0] * 18
aN=[0] * 18
for Noll in range (4,22):
    c0=c00[Noll-1]
    a0[Noll-4]=c0
    aN[Noll-4]=Noll

raw = tf.io.read_file(directory+'\hart.png')

image = tf.image.decode_png(raw, channels=3)
image = tf.abs(tf.cast(255 - image, dtype=tf.float32))
##################EfficientNetB0##################
inputs = layers.Input(shape=(224, 224, 3)) 
bottleneck = EfficientNetB0(weights=None, 
                            classifier_activation=None, 
                            include_top=False, 
                            pooling="avg") 
bottleneck = Model(inputs=bottleneck.inputs, 
                   outputs=bottleneck.layers[-2].output) 
bottleneck.trainable = False 
x = layers.GlobalAveragePooling2D(name="avg_pool")(bottleneck.output) 
x = layers.BatchNormalization()(x) 
top_dropout_rate = 0.2 
x = layers.Dropout(top_dropout_rate, name="top_dropout")(x) 
outputs = layers.Dense(18, activation=None, name="pred")(x) 
##################################################
# Compile 
start_time = time.time()
print("Выполнение ИНС начато: %s с " % (time.time() - start_time))
model = Model(bottleneck.input, outputs, name="EfficientNet")
with open(directory+'\\networks\cnn.txt', encoding='utf-8') as file:
    weightsPath = file.read()
weightsPath=weightsPath.replace('\n', '')
print(weightsPath)
model.load_weights(weightsPath)
a = model.predict(tf.expand_dims(image, axis=0))
print("Выполнение ИНС завершено. Время выполнения: %s с " % (time.time() - start_time))
print("Predicted zernike coefficients")
print(a[0])

RMS0=round((sum(np.array(a0   )**2))**0.5,3)   #simulated RMS
RMSa=round((sum(np.array(a[0] )**2))**0.5,3)   #predict RMS

F4=Begin(size,wavelength,N)
F4=CircAperture(F4,size/2,0,0)
for Noll in range (4,22):
    c00=a[0,Noll-4]
    #print(c00)
    (nz,mz)=noll_to_zern(Noll)
    #print(nz,mz)
    F4=Zernike(F4,nz,mz,size/2,c00,norm='False', units='lam')
F4=CircAperture(F4,size/2,0,0)
Phi4=Phase(F4)

with open(directory+'\\test_object.txt', encoding='utf-8') as file:
    testObjectPath = file.read()
testObjectPath=testObjectPath.replace('\n', '')
test_obj = imageio.imread(testObjectPath)
test_obj = cv2.resize(test_obj, (224, 224))

def ft2(g, delta):
    N=len(g)
    G = np.array(np.fft.fftshift(np.fft.fft2(g)))*(N*delta)**2
    return G
def ift2(g, delta):
    N=len(g)
    G = np.array(np.fft.ifftshift(np.fft.ifft2(g)))*(N*delta)**2
    return G
def myconv2(G, g, delta):
    N=len(g)
    Gg=np.array(ft2(G, delta)) * np.array(ft2(g, delta))
    C = ift2(Gg, 1/(N*delta))
    return C

# complex pupil function
F_00=Begin(size,wavelength,N)
F_00=CircAperture(F_00,size/2,0,0)
P0=Intensity(F_00, 1)
Phi2=np.load(directory+'\Phi2.npy')

Wexp1=np.array(Phi2)*(-1)*complex(0,1)
P1=np.array(P0)*np.array(np.exp(Wexp1))

Wexp2=np.array(Phi2-Phi4)*(-1)*complex(0,1)
P2=np.array(P0)*np.array(np.exp(Wexp2))

Wexp3=np.array(Phi4)*(-1)*complex(0,1)
P3=np.array(P0)*np.array(np.exp(Wexp3))

h0=ft2(P0, size/N)    #PSF ideal
h1=ft2(P1, size/N)    #PSF with simulated aberrations (Phi2)
h2=ft2(P2, size/N)    #PSF with corected phase (Phi2-Phi4)
h3=ft2(P3, size/N)    #PSF with AI reconstructed aberrations (Phi4)

g1=np.power(abs(test_obj), 1)
g2=np.power(abs(h0), 1)
g3=np.power(abs(h1), 1)
g4=np.power(abs(h2), 1)

img0=myconv2(g1, g2, 1)
img1=myconv2(g1, g3, 1)
img2=myconv2(g1, g4, 1)

# Deconvolution 
Freal=ft2(abs(img1),size/N)    #FFT real image
Freal_dm=ft2(abs(img2),size/N) #FFT real image with DM/SLM

F00=ft2(abs(h0),size/N)   #FFT PSF ideal
F01=ft2(abs(h1),size/N)   #FFT PSF with simulated aberrations (Phi2)
F02=ft2(abs(h2),size/N)   #FFT PSF with corected phase (Phi2-Phi4)
F03=ft2(abs(h3),size/N)   #FFT PSF with AI predicted aberrations (Phi4)

F_01=np.array(Freal)/np.array(F01)    # with simulated aberrations (Phi2)
F_02=np.array(Freal)/np.array(F00)    # without phase correction
F_03=np.array(Freal_dm)/np.array(F00) # with phase correction
F_04=np.array(Freal)/np.array(F03)    # with AI predicted aberrations (Phi2)

imgd1=ift2(F_01, size/N)    #image with deconvolution (Phi2) - verification
imgd2=ift2(F_02, size/N)    #image without without phase correction
imgd3=ift2(F_03, size/N)    #image PSF with corected phase (Phi2-Phi4)
imgd4=ift2(F_04, size/N)    #image PSF with AI predicted aberrations (Phi4)

plt.imsave(directory+'\Phi4.png', Phi4, cmap='jet')
plt.imsave(directory+'\deltaPhi.png', Phi4-Phi2, cmap='jet')
plt.imsave(directory+'\deconvolution_0.png', abs(imgd1), cmap='jet')
plt.imsave(directory+'\deconvolution_without_correction.png', abs(imgd2), cmap='jet')
plt.imsave(directory+'\deconvolution_dm.png', abs(imgd3), cmap='jet')
plt.imsave(directory+'\deconvolution_ai.png', abs(imgd4), cmap='jet')

plt.imsave(directory+'\image_0.png', abs(img0), cmap='jet')
plt.imsave(directory+'\image_without_correction.png', abs(img1), cmap='jet')
plt.imsave(directory+'\image_dm_correction.png', abs(img2), cmap='jet')

px = 1/plt.rcParams['figure.dpi']
plt.subplots(figsize=(2*224*px, 224*px))
plt.plot(aN,a0,aN,a[0])
plt.xlabel('Number of coef. Zernike')
plt.ylabel('Coef. Zernike')
plt.legend(['Simulated    RMS={}'.format(RMS0),'Predicted AI RMS={}'.format(RMSa)], framealpha=0.5)
plt.grid(which='major')
plt.grid(which='minor', linestyle=':')
plt.xlim([4, 21])
plt.ylim([-RMSa, RMSa])
plt.tight_layout()
plt.savefig(directory+'\Zernike.png')
#plt.show()
#plt.close()