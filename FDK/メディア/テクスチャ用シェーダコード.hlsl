// �萔�o�b�t�@�̃f�[�^��`
cbuffer cbCBuffer : register(b0)
{ // ��ɃX���b�g�u0�v���g��
    matrix World;      // ���[���h�ϊ��s��
    matrix View;       // �r���[�ϊ��s��
    matrix Projection; // �����ϊ��s��
    float TexLeft;     // �`�挳��`�̍�u���W
    float TexTop;      // �`�挳��`�̏�v���W
    float TexRight;    // �`�挳��`�̉Eu���W
    float TexBottom;   // �`�挳��`�̉�v���W
    float TexAlpha;    // �e�N�X�`���S�̂ɏ悶��A���t�@�l(0�`1)
};

Texture2D myTex2D; // �e�N�X�`��

// �T���v��
SamplerState smpWrap : register(s0);

// �s�N�Z���V�F�[�_�̓��̓f�[�^��`
struct PS_INPUT
{
    float4 Pos : SV_POSITION; // ���_���W(�������W�n)
    float2 Tex : TEXCOORD0;   // �e�N�X�`�����W
};

// ���_�V�F�[�_�̊֐�
PS_INPUT VS(uint vID : SV_VertexID)
{
    PS_INPUT vt;
    
    // ���_���W�i���f�����W�n�j�̐���
    switch (vID)
    {
        case 0:
            vt.Pos = float4(-0.5, 0.5, 0.0, 1.0); // ����
            vt.Tex = float2(TexLeft, TexTop);
            break;
        case 1:
            vt.Pos = float4(0.5, 0.5, 0.0, 1.0); // �E��
            vt.Tex = float2(TexRight, TexTop);
            break;
        case 2:
            vt.Pos = float4(-0.5, -0.5, 0.0, 1.0); // ����
            vt.Tex = float2(TexLeft, TexBottom);
            break;
        case 3:
            vt.Pos = float4(0.5, -0.5, 0.0, 1.0); // �E��
            vt.Tex = float2(TexRight, TexBottom);
            break;
    }

    // ���[���h�E�r���[�E�ˉe�ϊ�
    vt.Pos = mul(vt.Pos, World);
    vt.Pos = mul(vt.Pos, View);
    vt.Pos = mul(vt.Pos, Projection);

    // �o��
    return vt;
}

// �s�N�Z���V�F�[�_�̊֐�
float4 PS(PS_INPUT input) : SV_TARGET
{
	// �e�N�X�`���擾
	float4 texCol = myTex2D.Sample(smpWrap, input.Tex); // �e�N�Z���ǂݍ���
	texCol.a *= TexAlpha; // �A���t�@����Z

	// �F
	return saturate(texCol);
}
