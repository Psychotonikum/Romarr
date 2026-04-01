# Note, this script is only used for local dev tests, this is not the script used for building the official romarr package

mkdir -p /${PWD}/../_output_debian

docker build -f docker-build/Dockerfile -t romarr-packager ./docker-build

docker run --rm -v /${PWD}/../_output_linux:/data/romarr_bin:ro -v /${PWD}:/data/build -v /${PWD}/../_output_debian:/data/output romarr-packager
