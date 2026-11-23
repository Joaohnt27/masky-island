# Câmera da cena de encerramento

A cena Ending não possuía uma Camera: o vídeo era apresentado por RawImage em Canvas ScreenSpaceOverlay, com VideoPlayer em modo APIOnly. Foi adicionada a EndingDisplayCamera, ativa desde o carregamento da cena, direcionada ao Display 1 (índice 0), com fundo preto e culling mask vazio. Assim o display possui uma câmera de renderização, inclusive enquanto o vídeo é preparado e nas faixas fora da imagem.

A câmera pertence à cena e é destruída na troca para os créditos. Não foi adicionado outro AudioListener. O vídeo, sua proporção, seu áudio e o botão de pular continuam usando os componentes existentes.

Arquivos modificados: Assets/Scenes/Ending.unity.

Arquivos criados: Tests/UIAuthoring/FixEndingDisplay.cs, Tests/GameplayValidation/EndingDisplayValidation.cs e este relatório com os resultados da validação.

Configuração manual: nenhuma. Assets faltantes: nenhum.

A validação em Play Mode verifica a câmera desde o primeiro frame, saída para Display 1, ausência de AudioListener duplicado, frames reais do vídeo, textura ligada à interface e botão de pular levando aos créditos. A verificação é funcional em batch mode; não captura o aviso sobreposto da janela Game do Editor.

Backup da cena anterior: Builds/EndingDisplayBackup/Ending.unity.
