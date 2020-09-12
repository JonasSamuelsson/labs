|    Method |      SystemUnderTest |                    type |          Mean |      Error |     StdDev |        Median |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------- |--------------------- |------------------------ |--------------:|-----------:|-----------:|--------------:|-------:|-------:|------:|----------:|
| Attribute |          WithCaching |      IDontHaveAttribute |    29.9949 ns |  0.4942 ns |  0.4127 ns |    30.0594 ns |      - |      - |     - |         - |
| Interface |          WithCaching | IDontImplementInterface |    30.4782 ns |  0.5049 ns |  0.4216 ns |    30.4132 ns |      - |      - |     - |         - |
| Attribute |          WithCaching |          IHaveAttribute |    31.9133 ns |  0.4319 ns |  0.4040 ns |    31.9690 ns |      - |      - |     - |         - |
| Interface |          WithCaching |     IImplementInterface |    29.8848 ns |  0.3486 ns |  0.3260 ns |    29.8189 ns |      - |      - |     - |         - |
|     Class |          WithCaching |                 IsClass |     0.0329 ns |  0.0127 ns |  0.0106 ns |     0.0315 ns |      - |      - |     - |         - |
|     Class |          WithCaching |              IsNotClass |     0.0017 ns |  0.0064 ns |  0.0071 ns |     0.0000 ns |      - |      - |     - |         - |
| Attribute |       WithoutCaching |      IDontHaveAttribute |   426.9944 ns |  3.2874 ns |  2.7451 ns |   426.7506 ns |      - |      - |     - |         - |
| Interface |       WithoutCaching | IDontImplementInterface |    44.5763 ns |  0.7229 ns |  0.6037 ns |    44.6680 ns | 0.0204 | 0.0001 |     - |      32 B |
| Attribute |       WithoutCaching |          IHaveAttribute | 1,511.6007 ns | 24.3810 ns | 29.0238 ns | 1,500.8552 ns | 0.1221 |      - |     - |     192 B |
| Interface |       WithoutCaching |     IImplementInterface |    45.8089 ns |  0.7767 ns |  0.6485 ns |    45.8868 ns | 0.0204 | 0.0001 |     - |      32 B |
|     Class |       WithoutCaching |                 IsClass |     0.0529 ns |  0.0154 ns |  0.0144 ns |     0.0481 ns |      - |      - |     - |         - |
|     Class |       WithoutCaching |              IsNotClass |     0.0000 ns |  0.0000 ns |  0.0000 ns |     0.0000 ns |      - |      - |     - |         - |