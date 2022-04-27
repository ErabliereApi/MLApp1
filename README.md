# MLApp1

My experimentation using ML.Net to classify images of a camera system used in mapple syrop farm.

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

To configure the API url to match your environment change the static variable in ```MLApp1/Config.cs```.

## Setup the data source

This app analyse images from DAT file on a hard drive mount on V:. Those DAT file was saved there by a Smoonet camera system so the folder structure and the file name is as store by the camera recorder.