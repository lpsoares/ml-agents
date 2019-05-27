#!/bin/bash

RUN_ID=$1

#docker run --runtime=nvidia --name RollerBall --rm -ti --mount type=bind,source="$(pwd)"/unity-volume,target=/unity-volume -p 5005:5005 unity/ml-agents:gpu-latest mlagents-learn --docker-target-name=unity-volume config/trainer_config.yaml --train --run-id=RollerBall-1

# Run from executable
# docker run --runtime=nvidia --rm --name RollerBall --env="DISPLAY" --env="QT_X11_NO_MITSHM=1" --volume="/tmp/.X11-unix:/tmp/.X11-unix:rw"  --mount type=bind,source="$(pwd)"/unity-volume,target=/unity-volume -p 5005:5005 --workdir=/unity-volume unity/ml-agents:gpu-latest mlagents-learn  config/trainer_config.yaml --docker-target-name=unity-volume --env=env/RollerBall.x86_64 --train --run-id=rollerball_first_gpu_trial

# Run from Editor
docker run --runtime=nvidia --rm --name RollerBall --env="DISPLAY" --env="QT_X11_NO_MITSHM=1" --volume="/tmp/.X11-unix:/tmp/.X11-unix:rw"  --volume="$PWD/config":/unity-volume/config --mount type=bind,source="$(pwd)"/unity-volume,target=/unity-volume -p 5005:5005 --workdir=/unity-volume unity/ml-agents:gpu-latest mlagents-learn  config/trainer_config.yaml --docker-target-name=unity-volume  --train --run-id=$RUN_ID
