﻿using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Features.Consensus.Rules.CommonRules;
using Stratis.Bitcoin.Features.Consensus.Rules.ProvenHeaderRules;
using Stratis.Bitcoin.Features.MemoryPool.Rules;
using Stratis.Bitcoin.Networks.Deployments;
using Stratis.Bitcoin.Networks.Policies;

namespace Stratis.Bitcoin.Networks
{
    public class StraxMain : Network
    {
        public StraxMain()
        {
            this.Name = "StraxMain";
            this.NetworkType = NetworkType.Mainnet;
            this.Magic = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("StrX"));
            this.DefaultPort = 17105;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 100;
            this.DefaultRPCPort = 17104;
            this.DefaultAPIPort = 17103;
            this.DefaultSignalRPort = 17102;
            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = 10000;
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = StraxNetworkConstants.StraxRootFolderName;
            this.DefaultConfigFilename = StraxNetworkConstants.StraxDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "STRAX";
            this.DefaultBanTimeSeconds = 11250; // 500 (MaxReorg) * 45 (TargetSpacing) / 2 = 3 hours, 7 minutes and 30 seconds

            // TODO: Update this later
            this.FederationMultisigScript = new Script();

            var consensusFactory = new PosConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1598918400; // 1 September 2020
            this.GenesisNonce = 128917; // TODO: Update this once the final block is mined
            this.GenesisBits = 0x1e0fffff; // The difficulty target
            this.GenesisVersion = 1;
            this.GenesisReward = Money.Zero;

            // TODO: Update the genesis text to a dated newspaper article closer to launch
            Block genesisBlock = CreateStraxGenesisBlock(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward, "straxgenesisblock");

            this.Genesis = genesisBlock;

            // Taken from Stratis.
            var consensusOptions = new PosConsensusOptions(
                maxBlockBaseSize: 1_000_000,
                maxStandardVersion: 2,
                maxStandardTxWeight: 100_000,
                maxBlockSigopsCost: 20_000,
                maxStandardTxSigopsCost: 20_000 / 5,
                witnessScaleFactor: 4
            );

            var buriedDeployments = new BuriedDeploymentsArray
            {
                [BuriedDeployments.BIP34] = 0,
                [BuriedDeployments.BIP65] = 0,
                [BuriedDeployments.BIP66] = 0
            };

            var bip9Deployments = new StraxBIP9Deployments()
            {
                // Always active.
                [StraxBIP9Deployments.CSV] = new BIP9DeploymentsParameters("CSV", 0, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold),
                [StraxBIP9Deployments.Segwit] = new BIP9DeploymentsParameters("Segwit", 1, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold),
                [StraxBIP9Deployments.ColdStaking] = new BIP9DeploymentsParameters("ColdStaking", 2, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold)
            };

            this.Consensus = new NBitcoin.Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 105105, // https://github.com/satoshilabs/slips/blob/master/slip-0044.md
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: bip9Deployments,
                bip34Hash: null,
                minerConfirmationWindow: 2016,
                maxReorgLength: 500,
                defaultAssumeValid: null, // TODO: Set this once some checkpoint candidates have elapsed
                maxMoney: long.MaxValue,
                coinbaseMaturity: 50,
                premineHeight: 2,
                premineReward: Money.Coins(130000000),
                proofOfWorkReward: Money.Coins(18),
                powTargetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60),
                targetSpacing: TimeSpan.FromSeconds(45),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 12500,
                proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.Coins(18)
            );

            this.Consensus.PosEmptyCoinbase = false;

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { 75 }; // X
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { 140 }; // y
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (75 + 128) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
            };

            this.Bech32Encoders = new Bech32Encoder[2];
            var encoder = new Bech32Encoder("strax");
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.DNSSeeds = new List<DNSSeedData>
            {
            };

            this.SeedNodes = new List<NetworkAddress>
            {
            };

            this.StandardScriptsRegistry = new StratisStandardScriptsRegistry();

            Assert(this.DefaultBanTimeSeconds <= this.Consensus.MaxReorgLength * this.Consensus.TargetSpacing.TotalSeconds / 2);

            // TODO: Update these when the final block is mined
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x0000016c94cd31b3b84465335f7ceca6491ae6b54c7b8fafc6862e78b090ebe3"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("0xaa489ed22a8d82af81f0f124a16d9c61657b5fcf2708e0aecf3f78bbc6afb2ab"));

            this.RegisterRules(this.Consensus);
            this.RegisterMempoolRules(this.Consensus);
        }

        protected void RegisterRules(IConsensus consensus)
        {
            consensus.ConsensusRules
                .Register<HeaderTimeChecksRule>()
                .Register<HeaderTimeChecksPosRule>()
                .Register<PosFutureDriftRule>()
                .Register<CheckDifficultyPosRule>()
                .Register<StratisHeaderVersionRule>()
                .Register<ProvenHeaderSizeRule>()
                .Register<ProvenHeaderCoinstakeRule>();

            consensus.ConsensusRules
                .Register<BlockMerkleRootRule>()
                .Register<PosBlockSignatureRepresentationRule>()
                .Register<PosBlockSignatureRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsPartialValidationRule>()
                .Register<PosTimeMaskRule>()

                // rules that are inside the method ContextualCheckBlock
                .Register<TransactionLocktimeActivationRule>()
                .Register<CoinbaseHeightActivationRule>()
                .Register<WitnessCommitmentsRule>()
                .Register<BlockSizeRule>()

                // rules that are inside the method CheckBlock
                .Register<EnsureCoinbaseRule>()
                .Register<CheckPowTransactionRule>()
                .Register<CheckPosTransactionRule>()
                .Register<CheckSigOpsRule>()
                .Register<StraxCoinstakeRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsFullValidationRule>()

                .Register<CheckDifficultyHybridRule>()

                // rules that require the store to be loaded (coinview)
                .Register<LoadCoinviewRule>()
                .Register<TransactionDuplicationActivationRule>()
                .Register<StraxCoinviewRule>() // implements BIP68, MaxSigOps and BlockReward calculation
                                             // Place the PosColdStakingRule after the PosCoinviewRule to ensure that all input scripts have been evaluated
                                             // and that the "IsColdCoinStake" flag would have been set by the OP_CHECKCOLDSTAKEVERIFY opcode if applicable.
                .Register<StraxColdStakingRule>()
                .Register<SaveCoinviewRule>();
        }

        protected void RegisterMempoolRules(IConsensus consensus)
        {
            consensus.MempoolRules = new List<Type>()
            {
                typeof(CheckConflictsMempoolRule),
                typeof(CheckCoinViewMempoolRule),
                typeof(CreateMempoolEntryMempoolRule),
                typeof(CheckSigOpsMempoolRule),
                typeof(CheckFeeMempoolRule),
                typeof(CheckRateLimitMempoolRule),
                typeof(CheckAncestorsMempoolRule),
                typeof(CheckReplacementMempoolRule),
                typeof(CheckAllInputsMempoolRule),
                typeof(CheckTxOutDustRule)
            };
        }

        protected Block CreateStraxGenesisBlock(ConsensusFactory consensusFactory, uint time, uint nonce, uint bits, int version, Money genesisReward, string genesisText)
        {
            Transaction txNew = consensusFactory.CreateTransaction();
            txNew.Version = 1;
            txNew.Time = time;
            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(Op.GetPushOp(0), new Op()
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)42 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(genesisText)))
            });
            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
            });

            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(time);
            genesis.Header.Bits = bits;
            genesis.Header.Nonce = nonce;
            genesis.Header.Version = version;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();

            /*
            Procedure for creating a new genesis block:
            1. Create the template block as above in the CreateStraxGenesisBlock method

            3. Iterate over the nonce until the proof-of-work is valid

            while (!genesis.CheckProofOfWork())
            {
                genesis.Header.Nonce++;
                if (genesis.Header.Nonce == 0)
                    genesis.Header.Time++;
            }

            4. This will mean the block header hash is under the target
            5. Retrieve the Nonce and Time values from the resulting block header and insert them into the network definition
            */

            return genesis;
        }
    }
}