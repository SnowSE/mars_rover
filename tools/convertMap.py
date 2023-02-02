# requirements: python -m pip install imageio opencv-python
import os
import imageio.v2 as imageio
import cv2
import numpy as np
from argparse import ArgumentParser

def intensities_to_damages(intensities):
    intensities = np.array(intensities, dtype=np.int64)
    out = np.zeros_like(intensities)
    num_rows, num_columns = out.shape
    for row in range(1, num_rows - 1):
        for column in range(1, num_columns - 1):
            out[row, column] = sum([
                abs(intensities[row - 1, column] - intensities[row, column]),
                abs(intensities[row, column - 1] - intensities[row, column]),
                abs(intensities[row + 1, column] - intensities[row, column]),
                abs(intensities[row, column + 1] - intensities[row, column]),
            ])
    return out

def test_intensities_to_damages():
    map = np.array([
        [1, 2, 5, 3],
        [1, 3, 6, 4],
        [0, 3, 6, 5],
        [1, 5, 4, 7]
    ])
    v1 = abs(3-2) + abs(3-6) + abs(3-3) + abs(3-1)
    v2 = abs(6-5) + abs(6-4) + abs(6-6) + abs(6-3)
    v3 = abs(3-3) + abs(3-6) + abs(3-5) + abs(3-0)
    v4 = abs(6-6) + abs(6-5) + abs(6-4) + abs(6-3)
    expected = np.array([
        [0, 0, 0, 0],
        [0, v1, v2, 0],
        [0, v3, v4, 0],
        [0, 0, 0, 0],
    ])
    observed = intensities_to_damages(map)
    assert (expected == observed).all()

def test_intensities_to_damages_unsigned():
    map = np.array([
        [1, 2, 5, 3],
        [1, 3, 6, 4],
        [0, 3, 6, 5],
        [1, 5, 4, 7]
    ], dtype=np.uint)
    v1 = abs(3-2) + abs(3-6) + abs(3-3) + abs(3-1)
    v2 = abs(6-5) + abs(6-4) + abs(6-6) + abs(6-3)
    v3 = abs(3-3) + abs(3-6) + abs(3-5) + abs(3-0)
    v4 = abs(6-6) + abs(6-5) + abs(6-4) + abs(6-3)
    expected = np.array([
        [0, 0, 0, 0],
        [0, v1, v2, 0],
        [0, v3, v4, 0],
        [0, 0, 0, 0],
    ])
    observed = intensities_to_damages(map)
    assert (expected == observed).all()

def main():
    parser = ArgumentParser('convertMap', description="Converts image file to json damage map (list of lists).")
    parser.add_argument('--file', '-f', help="path to input image file (.jpg, .png, etc.)", default='../resources/map01.jpg')
    parser.add_argument('--size', '-s', type=int, help="desired output height and width (in pixels) [defaults to original dimensions]")
    parser.add_argument('--out-type', '-o', choices=['json', 'csv'], default='json', help="desired format for output file")
    args = parser.parse_args()
    filename_prefix, _ = os.path.splitext(args.file)
    image = imageio.imread(args.file)
    if args.size is not None:
        image = cv2.resize(image, (args.size, args.size))
    damages = intensities_to_damages(image.sum(-1))

    if args.out_type == 'json':
        out_contents = str(damages.tolist())
    else:
        out_contents = '\n'.join([','.join(map(str, line)) for line in damages.tolist()])

    with open(f"{filename_prefix}.{args.out_type}", 'w+') as outfile:
        print(out_contents, file=outfile)

if __name__ == '__main__':
    main()

