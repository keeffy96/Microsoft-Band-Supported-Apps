package com.microsoft.band.sdk.sampleapp.accelerometer;
import com.microsoft.band.BandClient;
import com.microsoft.band.BandClientManager;
import com.microsoft.band.BandException;
import com.microsoft.band.BandInfo;
import com.microsoft.band.ConnectionState;
import com.microsoft.band.sensors.BandAccelerometerEvent;
import com.microsoft.band.sensors.BandAccelerometerEventListener;
import com.microsoft.band.sensors.SampleRate;


import android.media.AudioAttributes;
import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.media.SoundPool;
import android.os.Build;
import android.os.Bundle;
import android.view.View;
import android.app.Activity;
import android.os.AsyncTask;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.RadioButton;
import android.widget.SeekBar;
import android.widget.TextView;

public class BandAccelerometerAppActivity extends Activity {

	private SoundPool soundPool;
	private AudioManager audioManager;
	private static final int MAX_STREAMS = 5;
	public static final int streamType = AudioManager.STREAM_MUSIC;
	public boolean loaded;
	private int soundTechno;
	private float Volume;
	int amp = 10000;
	double twopi = 8.*Math.atan(1.);
	double fr = 440.f;
	double ph = 0.0 , ph1 = 0.0;
	int Dsamps;

	Thread t;
	int sr = 44100;
	boolean isRunning = true;
	short samples[];
	int buffsize;
	AudioTrack audioTrack;

	private BandClient client = null;
	private Button btnStart, btnStop;
	private TextView txtStatus;
	double xVal,yVal,zVal;

	private BandAccelerometerEventListener mAccelerometerEventListener = new BandAccelerometerEventListener() {
		@Override
		public void onBandAccelerometerChanged(final BandAccelerometerEvent event) {
			if (event != null) {
				xVal=event.getAccelerationX();
				yVal=event.getAccelerationY();
				zVal=event.getAccelerationZ();
				xVal = Math.round(Math.abs(xVal*1000));
				yVal = Math.round(Math.abs(yVal*10));
				zVal = Math.abs(zVal*10);

				appendToUI(String.format("Frequency = %.0f hz \n Vibrato = %.0f \n Rate = %.1f ", xVal, yVal,zVal));
			}
		}
	};

	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);

		txtStatus = (TextView) findViewById(R.id.txtStatus);
		btnStart = (Button) findViewById(R.id.btnStart);

		btnStart.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				txtStatus.setText("");
				new AccelerometerSubscriptionTask().execute();
				audioTrack.play();
			}
		});

		btnStop = (Button) findViewById(R.id.btnStop);

		btnStop.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				audioTrack.pause();
			}
		});

		t = new Thread() {
			public void run() {
				// set process priority
				setPriority(Thread.MAX_PRIORITY);
				buffsize = AudioTrack.getMinBufferSize(sr,
						AudioFormat.CHANNEL_OUT_MONO, AudioFormat.ENCODING_PCM_16BIT);
				audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC,
						sr, AudioFormat.CHANNEL_OUT_MONO,
						AudioFormat.ENCODING_PCM_16BIT, buffsize,
						AudioTrack.MODE_STREAM);

				Dsamps=buffsize;
				audioTrack.play();
				samples = new short[buffsize];
				RadioButton radioButton1, radioButton2,radioButton3, radioButton4, radioButton5;
				radioButton1 = (RadioButton) findViewById(R.id.radioButton1);
				radioButton2 = (RadioButton) findViewById(R.id.radioButton2);
				radioButton3 = (RadioButton) findViewById(R.id.radioButton3);
				radioButton4 = (RadioButton) findViewById(R.id.radioButton4);
				radioButton5 = (RadioButton) findViewById(R.id.radioButton5);

				while(isRunning)
				{
					if(radioButton1.isChecked())
					{
						sine(samples, buffsize);
						audioTrack.write(samples, 0, buffsize);
					}

					else if(radioButton2.isChecked())
					{
						sineRmod(samples, buffsize);
						audioTrack.write(samples, 0, buffsize);
					}

					else if(radioButton3.isChecked())
					{
						sineFmod(samples, buffsize);
						audioTrack.write(samples, 0, buffsize);
					}

					else if(radioButton4.isChecked())
					{
						overDrive(samples, buffsize);
						audioTrack.write(samples, 0, buffsize);
					}

					else if(radioButton5.isChecked())
					{
						square(samples, buffsize);
						audioTrack.write(samples, 0, buffsize);
					}

				}
			}
		};
		t.start();

		audioManager = (AudioManager) getSystemService(AUDIO_SERVICE);
		float currentVolumeIndex = (float) audioManager.getStreamVolume(streamType);
		float maxVolumeIndex  = (float) audioManager.getStreamMaxVolume(streamType);
		this.Volume = currentVolumeIndex / maxVolumeIndex;
		this.setVolumeControlStream(streamType);
		if (Build.VERSION.SDK_INT >= 21 )
		{
			AudioAttributes audioAttrib = new AudioAttributes.Builder()
					.setUsage(AudioAttributes.USAGE_GAME)
					.setContentType(AudioAttributes.CONTENT_TYPE_SONIFICATION)
					.build();

			SoundPool.Builder builder= new SoundPool.Builder();
			builder.setAudioAttributes(audioAttrib).setMaxStreams(MAX_STREAMS);

			this.soundPool = builder.build();
		}

		else
		{
			this.soundPool = new SoundPool(MAX_STREAMS, AudioManager.STREAM_MUSIC, 0);
		}

		this.soundPool.setOnLoadCompleteListener(new SoundPool.OnLoadCompleteListener()
		{
			public void onLoadComplete(SoundPool soundPool, int sampleId, int status) {
				loaded = true;
			}
		});

		this.soundTechno = this.soundPool.load(this, R.raw.sound6,1);
	}

	public void playMusic(View view)  {
		if(loaded)  {
			float leftVolume = Volume;
			float rightVolume = Volume;
			soundPool.play(soundTechno,leftVolume, rightVolume, 1, 10, (float)zVal);
		}
	}

	public short[] sine(short[] samples, int buffsize) {
		fr = xVal; //xVal= frequency

		for (int i = 0; i < buffsize; i++) {
			samples[i] = (short) (amp * 0.5 * Math.sin(ph));
			ph += twopi * fr / sr;
		}
		return samples;
	}

	public short[] square(short[] samples, int buffsize){
		fr = xVal; //xVal= frequency

		for (int i = 0; i < buffsize; i++){
			samples[i] = (short) (amp * 0.5 * Math.signum(Math.sin(ph)));
			ph += twopi * fr / sr;
		}
		return samples;
	}

	//xVal = frequency, fr2(yVal) = vibrato
	public short[] sineRmod(short[] samples, int buffsize) {
		fr = xVal;
		double fr2=yVal;

		for (int i = 0; i < buffsize; i++) {
			samples[i] = (short) (amp * 0.5 * ((Math.cos(ph) * Math.sin(ph1))));
			ph1 += twopi * fr2/ sr;
			ph += twopi * fr / sr;
		}
		return samples;
	}

	public short[] sineFmod(short[] samples, int buffsize){
		//xVal = frequency yVal=
		fr = xVal;
		double fr2 = yVal/10;
		double modIndex=2;
		for (int i = 0; i < buffsize; i++){
			samples[i] = (short) (amp * 0.5 *(Math.cos(ph+modIndex*Math.sin(ph1))));
			ph += twopi * fr / sr;
			ph1 += twopi * (fr2) / sr;
		}
		return samples;
	}

	public short[] overDrive(short[] samples, int buffsize) {
		fr = xVal;
		//xVal causes the wave to become distorted
		double [] temp = new double[buffsize];

		for (int i = 0; i < buffsize; i++){
			temp[i] = 0.5 * Math.cos(ph);
			ph += twopi * fr / sr;
		}

		for (int i = 0; i < buffsize; i++) {
			if (Math.abs(temp[i]) <= 0.33333)
				samples[i] = (short) (amp*2 * temp[i]);
			else if (Math.abs(temp[i]) > 0.33333 && Math.abs(temp[i]) <= 0.66667)
				samples[i] = (short) (amp*(Math.signum(temp[i]) * 0.33333 * (3 - Math.pow(2 - 3 * Math.abs(temp[i]), 2))));
			else
				samples[i] = (short) (amp*Math.signum(temp[i]));
		}
		return samples;
	}





	private class AccelerometerSubscriptionTask extends AsyncTask<Void, Void, Void> {
		@Override
		protected Void doInBackground(Void... params) {
			try {
				if (getConnectedBandClient()) {
					appendToUI("Band is connected.\n");
					client.getSensorManager().registerAccelerometerEventListener(mAccelerometerEventListener, SampleRate.MS128);
				} else {
					appendToUI("Band isn't connected. Please make sure bluetooth is on and the band is in range.\n");
				}
			} catch (BandException e) {
				String exceptionMessage;
				switch (e.getErrorType()) {
					case UNSUPPORTED_SDK_VERSION_ERROR:
						exceptionMessage = "Microsoft Health BandService doesn't support your SDK Version. Please update to latest SDK.\n";
						break;
					case SERVICE_ERROR:
						exceptionMessage = "Microsoft Health BandService is not available. Please make sure Microsoft Health is installed and that you have the correct permissions.\n";
						break;
					default:
						exceptionMessage = "Unknown error occured: " + e.getMessage() + "\n";
						break;
				}
				appendToUI(exceptionMessage);

			} catch (Exception e) {
				appendToUI(e.getMessage());
			}
			return null;
		}
	}

	private void appendToUI(final String string) {
		this.runOnUiThread(new Runnable() {
			@Override
			public void run() {
				txtStatus.setText(string);
			}
		});
	}

	private boolean getConnectedBandClient() throws InterruptedException, BandException {
		if (client == null) {
			BandInfo[] devices = BandClientManager.getInstance().getPairedBands();
			if (devices.length == 0) {
				appendToUI("Band isn't paired with your phone.\n");
				return false;
			}
			client = BandClientManager.getInstance().create(getBaseContext(), devices[0]);
		} else if (ConnectionState.CONNECTED == client.getConnectionState()) {
			return true;
		}

		return ConnectionState.CONNECTED == client.connect().await();
	}
}

