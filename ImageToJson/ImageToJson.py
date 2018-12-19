
import cv2
import sys
import json
import os
import numpy as np

class ToJson:
    def printData(self):
        if self.img != None:
            for i in self.img:
                for j in i:
                    print(j)

    def getImageArray(self, img):
        for i in range(len(img)):
            for j in range(len(img[i])):
                img[i][j] = img[i][j][::-1]
        return img


    def saveToJson(self, name, imageData, type = "img", fps = 50):
        prevData = json.load(open("LPImages.json", "r"))
        newData = prevData
        if type == "img":
            newData[name] = {"type" : type,
                             "data" : imageData.tolist()}
        elif type == "gif":
            print(imageData)
            newData[name] = {"type" : type,
                                "fps" : fps,
                                "data" : imageData.tolist()}
        json.dump(newData, open("LPImages.json", "w"))

    def convertFromFolder(self, folder):
        print(os.listdir(folder))
        files = os.listdir(folder)
        for file in files:
            print(file[-4:])
            if file[-4:] == ".png":
                self.saveToJson(file[:-4], self.getImageArray(cv2.imread("convert\\{}".format(file),1)))
            elif file[-4:] == ".gif":
                self.saveToJson(file[:-4], self.getGifArray(cv2.VideoCapture("convert\\{}".format(file))), type="gif")
            else:
                print("File {} is not a .PNG or .GIF".format(file))
    def getGifArray(self, videoCapture):
        valid, gif = videoCapture.read()
        imageArray = []
        frame = 0
        while valid:
            if valid:
                imageArray.append(gif)

                valid, gif = videoCapture.read()
            #print("Frame: " + str(frame))
            frame +=1
        
        imageArray = np.asarray(imageArray)[:-1]
        for frame in range(len(imageArray)):
            for i in range(len(imageArray[frame])):
                for n in range(len(imageArray[frame][i])):
                    imageArray[frame][i][n] = imageArray[frame][i][n][::-1];

        videoCapture.release()
        return imageArray


converter = ToJson()
#converter.getImageArray()
#converter.saveToJson("Peen of suck", converter.getImageArray())


print("This image converter will convert all png files in the convert folder\ninto the LPImages json file. Place all images to be converted in\nthe convert folder along with making sure the LPImages.json file\nis present in the same folder as this script.")
print("If you wish to convert all images in the convert folder press y, otherwise anykey will escape the program")
key = input(">>")
if key.lower() == "y":
    print("Converting...")
    converter.convertFromFolder("convert")
    print("Converted")
else:
    print("Exiting program...")

#converter.getGifArray(cv2.VideoCapture("convert\\{}".format("Logo.gif")))