import imageio.v2 as imageio
import numpy as np

def intensities_to_damages(intensities):
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
    v2 = abs(6-5) + abs(6-4) + abs(6-6) + abs(6-4)
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
    image = imageio.imread('../resources/map01.jpg')
    intensities = image.sum(axis=-1)

if __name__ == '__main__':
    main()

