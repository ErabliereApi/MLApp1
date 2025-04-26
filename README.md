# MLApp1

My experimentation using ML.Net to classify images of a camera system used in maple syrup farm.

## Setup

To use the app you need to deploy an image2text api. You can do it using docker, a raspberry pi or many other way. Even unit test use the api to pass.

### Docker

Run this command

```
docker run -i -t -p 39000:5000 erabliereapi/extraireinfohmi:latest flask run --host=0.0.0.0
```

### Raspberry PI

An alternative is to deploy the api on a raspberry pi. To run the api as a background service run the following command.

```
sudo crontab -e
```

And add this line

```
@reboot sudo FLASK_APP=/home/ubuntu/erabliereapi/PythonScripts/image2textapi.py su -p - ubuntu -c 'flask run --host=0.0.0.0' >/var/log/image2text.log 2>&1
```

### Configure the API url

To configure the API url to match your environment change the static class in ```MLApp1/Config.cs```.

## Setup the data source

This app analyse images from DAT file on a hard drive mount on D:. Those DAT file was saved there by a Smonet camera system so the folder structure and the file name is as store by the camera recorder.

## Traning workflow

1. Run the MlApp1 projet using dotnet run. It will open a console app with the following choices.

```
1. Prepare folder
2. Train a model
3. Classify images
4. Classify image folder using image2text api
5. Classify image using ML.Net
6. Edit options
0. Quit
```

2. Choose 1 to prepare the folder. It will create a folder structure and copy the images from the DAT file to the folder structure. The folder structure is as follow:

The algorithm will proceded as follow:

1. In for loops
2. Parse a .dat file on the external hard drive
3. Getting images from the .dat file and storing then into a Temp folder inside the bin folder
4. Classify the images using the image2text api
5. Base on the classification result, move the images to the corresponding folder in the workspace folder
6. Images in the temp folder are deleted

