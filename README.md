❗❗❗This project is only for Unity 2022.3 LTS with URP❗❗❗

**Perception** is designed to empower Unity ML-Agents with advanced computer vision capabilities.
It allows a standard Unity Camera to generate rich Ground Truth data including **2D Bounding Boxes**, **Semantic Segmentation masks**, and **Gray-scale Depth maps**.

Perception is specifically engineered for runtime integration with **Unity ML-Agents**. It wraps these ground truth generators into custom Sensor Components, allowing Agents to directly observe the segmented labeled, and depth world.
The observations are serialized and sent to the external Python environment via ML-Agents API, enabling the training of complex vision-based reinforcement learning models.
The Python samples is available on [GitHub https://github.com/BlueFisher/Perception-for-ML-Agents-Sample/blob/master/Perception_unity_wrapper.ipynb](https://github.com/BlueFisher/Perception-for-ML-Agents-Sample/blob/master/Perception_unity_wrapper.ipynb).

Perception is a fork of the now-deprecated com.unity.perception. It strips out the dataset capture (offline data collection) features to focus on lightweight, real-time ground truth generation.

# Compatibility

- Unity 2022.3 LTS
- Universal Render Pipeline (URP)
- Unity ML-Agents 3.0.0 (Release 22) (This project includes a modified version of ML-Agents 3.0.0 since it is not available via Package Manager)

# Documentation

[English Documentation](https://github.com/BlueFisher/Perception-for-ML-Agents-Sample/blob/master/doc/Document_en.md)

[中文文档](https://github.com/BlueFisher/Perception-for-ML-Agents-Sample/blob/master/doc/Document_cn.md)